// Service Worker for M8L Set Card Game - Push Notifications & Offline Support
const CACHE_NAME = 'setm8l-cache-v1';

// Derive base path from the service worker's scope (works for both local dev and GitHub Pages)
function getBasePath() {
    // self.registration.scope gives us the full URL like
    // "https://user.github.io/SetM8L.BlazorApp/" or "http://localhost:12826/"
    try {
        const scopeUrl = new URL(self.registration.scope);
        return scopeUrl.pathname; // e.g. "/SetM8L.BlazorApp/" or "/"
    } catch {
        return '/';
    }
}

// Install event - cache essential assets
self.addEventListener('install', (event) => {
    self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames
                    .filter((name) => name !== CACHE_NAME)
                    .map((name) => caches.delete(name))
            );
        }).then(() => self.clients.claim())
    );
});

// Push event - display notification when a push message arrives
self.addEventListener('push', (event) => {
    const basePath = getBasePath();

    let data = {
        title: 'M8L Set Game',
        body: 'You have a new notification!',
        icon: basePath + 'icon-192.png',
        badge: basePath + 'icon-192.png',
        url: basePath,
        tag: 'default'
    };

    if (event.data) {
        try {
            const payload = event.data.json();
            data = {
                title: payload.title || data.title,
                body: payload.body || data.body,
                icon: payload.icon || data.icon,
                badge: payload.badge || data.badge,
                url: payload.url || data.url,
                tag: payload.tag || data.tag,
                data: payload.data || {}
            };
        } catch (e) {
            // If payload is plain text
            data.body = event.data.text();
        }
    }

    const options = {
        body: data.body,
        icon: data.icon,
        badge: data.badge,
        tag: data.tag,
        renotify: true,
        requireInteraction: false,
        data: {
            url: data.url,
            ...data.data
        }
    };

    // Must show a notification - iOS/browsers may revoke permission otherwise
    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});

// Notification click - open/focus the app to the right page
self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    const basePath = getBasePath();
    const targetUrl = event.notification.data?.url || basePath;

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            // Check if the app is already open
            for (const client of clientList) {
                if (client.url.includes(basePath) && 'focus' in client) {
                    // Navigate to the target URL and focus
                    client.navigate(targetUrl);
                    return client.focus();
                }
            }
            // App not open - open in standalone mode (iOS handles this automatically for installed PWAs)
            return clients.openWindow(targetUrl);
        })
    );
});

// Notification close event
self.addEventListener('notificationclose', (event) => {
    // Optional: track notification dismissals
});

// Fetch event - network-first strategy (minimal caching for a dynamic app)
self.addEventListener('fetch', (event) => {
    // Let Blazor and Firebase requests pass through normally
    // Only cache static assets if needed in the future
});
