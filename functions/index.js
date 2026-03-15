const functions = require('firebase-functions');
const admin = require('firebase-admin');
const webpush = require('web-push');

admin.initializeApp();
const db = admin.database();

// VAPID keys — set these via Firebase config:
//   firebase functions:config:set vapid.public_key="..." vapid.private_key="..." vapid.email="mailto:you@example.com"
const vapidConfig = functions.config().vapid || {};
const VAPID_PUBLIC_KEY = vapidConfig.public_key || '';
const VAPID_PRIVATE_KEY = vapidConfig.private_key || '';
const VAPID_EMAIL = vapidConfig.email || 'mailto:noreply@example.com';

if (VAPID_PUBLIC_KEY && VAPID_PRIVATE_KEY) {
    webpush.setVapidDetails(VAPID_EMAIL, VAPID_PUBLIC_KEY, VAPID_PRIVATE_KEY);
}

/**
 * Triggered when a room is updated in /rooms/{roomCode}.
 * Detects new players joining and sends push notifications to existing players.
 */
exports.onRoomUpdated = functions
    .region('europe-west1')
    .database.ref('/rooms/{roomCode}')
    .onUpdate(async (change, context) => {
        const roomCode = context.params.roomCode;
        const before = change.before.val();
        const after = change.after.val();

        if (!before || !after) return null;

        // Get player lists (stored as object keyed by playerId)
        const playersBefore = before.players || {};
        const playersAfter = after.players || {};

        const beforeIds = new Set(Object.keys(playersBefore));
        const afterIds = new Set(Object.keys(playersAfter));

        // Find newly joined players
        const newPlayerIds = [...afterIds].filter((id) => !beforeIds.has(id));

        if (newPlayerIds.length === 0) return null;

        // Don't notify if game already started (avoid noise during gameplay)
        if (after.isStarted) return null;

        // For each new player, notify all OTHER players in the room
        const notifications = [];

        for (const newPlayerId of newPlayerIds) {
            const newPlayer = playersAfter[newPlayerId];
            const newPlayerName = newPlayer.name || 'Someone';

            // Get all other players who were already in the room
            const playersToNotify = Object.keys(playersAfter).filter(
                (id) => id !== newPlayerId
            );

            for (const targetPlayerId of playersToNotify) {
                notifications.push(
                    sendPushToUser(targetPlayerId, {
                        title: 'Player Joined!',
                        body: `${newPlayerName} joined room ${roomCode}`,
                        icon: '/SetM8L.BlazorApp/icon-192.png',
                        url: `/SetM8L.BlazorApp/waiting/${roomCode}`,
                        tag: `room-${roomCode}-join`,
                        data: { roomCode, playerId: newPlayerId }
                    })
                );
            }
        }

        const results = await Promise.allSettled(notifications);
        const sent = results.filter((r) => r.status === 'fulfilled' && r.value === true).length;
        console.log(`Room ${roomCode}: ${newPlayerIds.length} new player(s), ${sent} notification(s) sent`);

        return null;
    });

/**
 * Send a push notification to a specific user by their player ID.
 * Reads their subscription from /pushSubscriptions/{userId}.
 */
async function sendPushToUser(userId, payload) {
    if (!VAPID_PUBLIC_KEY || !VAPID_PRIVATE_KEY) {
        console.warn('VAPID keys not configured. Run: firebase functions:config:set vapid.public_key="..." vapid.private_key="..." vapid.email="..."');
        return false;
    }

    try {
        const snapshot = await db.ref(`pushSubscriptions/${userId}`).once('value');
        const sub = snapshot.val();

        if (!sub || !sub.endpoint || !sub.keys) {
            // User has no push subscription — that's fine, not everyone enables notifications
            return false;
        }

        const pushSubscription = {
            endpoint: sub.endpoint,
            keys: {
                p256dh: sub.keys.p256dh,
                auth: sub.keys.auth
            }
        };

        await webpush.sendNotification(pushSubscription, JSON.stringify(payload));
        return true;
    } catch (error) {
        if (error.statusCode === 404 || error.statusCode === 410) {
            // Subscription expired or revoked — clean it up
            await db.ref(`pushSubscriptions/${userId}`).remove();
            console.log(`Removed expired subscription for user ${userId}`);
        } else {
            console.error(`Push to ${userId} failed:`, error.message);
        }
        return false;
    }
}
