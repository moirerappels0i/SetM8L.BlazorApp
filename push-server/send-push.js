/**
 * Push Notification Server for M8L Set Card Game
 *
 * This script reads push subscriptions from Firebase and sends
 * push notifications using the Web Push protocol.
 *
 * Usage:
 *   1. Generate VAPID keys:    npm run generate-vapid-keys
 *   2. Set environment vars:   VAPID_PUBLIC_KEY, VAPID_PRIVATE_KEY, VAPID_EMAIL
 *   3. Set Firebase config:    FIREBASE_DATABASE_URL
 *   4. Send a notification:    node send-push.js
 *
 * Environment Variables:
 *   VAPID_PUBLIC_KEY    - Your VAPID public key (base64url)
 *   VAPID_PRIVATE_KEY   - Your VAPID private key (base64url)
 *   VAPID_EMAIL         - Contact email for VAPID (mailto:you@example.com)
 *   FIREBASE_DATABASE_URL - Your Firebase Realtime Database URL
 *   FIREBASE_SERVICE_ACCOUNT - Path to Firebase service account JSON file
 */

const webpush = require('web-push');
const admin = require('firebase-admin');

// --- Configuration ---

const VAPID_PUBLIC_KEY = process.env.VAPID_PUBLIC_KEY || 'YOUR_VAPID_PUBLIC_KEY_HERE';
const VAPID_PRIVATE_KEY = process.env.VAPID_PRIVATE_KEY || 'YOUR_VAPID_PRIVATE_KEY_HERE';
const VAPID_EMAIL = process.env.VAPID_EMAIL || 'mailto:your-email@example.com';
const FIREBASE_DATABASE_URL = process.env.FIREBASE_DATABASE_URL || 'https://your-project.firebaseio.com';
const SERVICE_ACCOUNT_PATH = process.env.FIREBASE_SERVICE_ACCOUNT || './serviceAccountKey.json';

// Set VAPID details
webpush.setVapidDetails(VAPID_EMAIL, VAPID_PUBLIC_KEY, VAPID_PRIVATE_KEY);

// Initialize Firebase Admin
try {
    const serviceAccount = require(SERVICE_ACCOUNT_PATH);
    admin.initializeApp({
        credential: admin.credential.cert(serviceAccount),
        databaseURL: FIREBASE_DATABASE_URL
    });
} catch (e) {
    console.error('Firebase Admin initialization failed. Ensure service account file exists.');
    console.error('Download from: Firebase Console > Project Settings > Service accounts > Generate new private key');
    process.exit(1);
}

const db = admin.database();

/**
 * Send a push notification to all stored subscriptions.
 * @param {Object} payload - Notification payload
 * @param {string} payload.title - Notification title
 * @param {string} payload.body - Notification body text
 * @param {string} [payload.icon] - Notification icon URL
 * @param {string} [payload.url] - URL to open when notification is tapped
 * @param {string} [payload.tag] - Notification tag for grouping
 * @param {Object} [payload.data] - Additional custom data
 */
async function sendPushToAll(payload) {
    const defaultPayload = {
        title: 'M8L Set Game',
        body: 'You have a new notification!',
        icon: '/SetM8L.BlazorApp/icon-192.png',
        url: '/SetM8L.BlazorApp/',
        tag: 'game-notification',
        ...payload
    };

    try {
        // Read all subscriptions from Firebase
        const snapshot = await db.ref('pushSubscriptions').once('value');
        const subscriptions = snapshot.val();

        if (!subscriptions) {
            console.log('No subscriptions found.');
            return { sent: 0, failed: 0, removed: 0 };
        }

        let sent = 0;
        let failed = 0;
        let removed = 0;

        for (const [userId, sub] of Object.entries(subscriptions)) {
            const pushSubscription = {
                endpoint: sub.endpoint,
                keys: {
                    p256dh: sub.keys.p256dh,
                    auth: sub.keys.auth
                }
            };

            try {
                await webpush.sendNotification(
                    pushSubscription,
                    JSON.stringify(defaultPayload)
                );
                sent++;
                console.log(`Sent to user ${userId}`);
            } catch (error) {
                failed++;

                if (error.statusCode === 404 || error.statusCode === 410) {
                    // Subscription expired or revoked - remove it
                    await db.ref(`pushSubscriptions/${userId}`).remove();
                    removed++;
                    console.log(`Removed expired subscription for user ${userId}`);
                } else {
                    console.error(`Failed to send to user ${userId}:`, error.message);
                }
            }
        }

        const result = { sent, failed, removed };
        console.log('Push notification results:', result);
        return result;
    } catch (error) {
        console.error('Error sending push notifications:', error);
        throw error;
    }
}

/**
 * Send a push notification to a specific user.
 * @param {string} userId - The Firebase user ID
 * @param {Object} payload - Notification payload
 */
async function sendPushToUser(userId, payload) {
    const defaultPayload = {
        title: 'M8L Set Game',
        body: 'You have a new notification!',
        icon: '/SetM8L.BlazorApp/icon-192.png',
        url: '/SetM8L.BlazorApp/',
        tag: 'game-notification',
        ...payload
    };

    try {
        const snapshot = await db.ref(`pushSubscriptions/${userId}`).once('value');
        const sub = snapshot.val();

        if (!sub) {
            console.log(`No subscription found for user ${userId}`);
            return false;
        }

        const pushSubscription = {
            endpoint: sub.endpoint,
            keys: {
                p256dh: sub.keys.p256dh,
                auth: sub.keys.auth
            }
        };

        await webpush.sendNotification(
            pushSubscription,
            JSON.stringify(defaultPayload)
        );

        console.log(`Push sent to user ${userId}`);
        return true;
    } catch (error) {
        if (error.statusCode === 404 || error.statusCode === 410) {
            await db.ref(`pushSubscriptions/${userId}`).remove();
            console.log(`Removed expired subscription for user ${userId}`);
        } else {
            console.error(`Failed to send to user ${userId}:`, error.message);
        }
        return false;
    }
}

// --- CLI usage ---
// Run directly: node send-push.js "Title" "Body" "/path/to/open"

if (require.main === module) {
    const title = process.argv[2] || 'Game Update';
    const body = process.argv[3] || 'Something happened in your game!';
    const url = process.argv[4] || '/SetM8L.BlazorApp/';

    sendPushToAll({ title, body, url })
        .then((result) => {
            console.log('Done:', result);
            process.exit(0);
        })
        .catch((error) => {
            console.error('Failed:', error);
            process.exit(1);
        });
}

module.exports = { sendPushToAll, sendPushToUser };
