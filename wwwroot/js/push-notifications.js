// Push Notification Interop for M8L Set Card Game
// Handles service worker registration, push subscription, and standalone mode detection

window.PushNotifications = {
    _swRegistration: null,
    _subscription: null,

    // Derive base path from the <base> tag (works for both local dev and GitHub Pages)
    _getBasePath: function () {
        const baseEl = document.querySelector('base');
        if (baseEl && baseEl.getAttribute('href')) {
            return baseEl.getAttribute('href'); // e.g. "/" or "/SetM8L.BlazorApp/"
        }
        return '/';
    },

    // Initialize - register service worker
    initialize: async function () {
        if (!('serviceWorker' in navigator)) {
            console.warn('Service workers not supported');
            return false;
        }

        try {
            const basePath = this._getBasePath();
            this._swRegistration = await navigator.serviceWorker.register(basePath + 'sw.js', {
                scope: basePath
            });
            console.log('Service worker registered:', this._swRegistration.scope);

            // Wait for the service worker to be ready
            await navigator.serviceWorker.ready;
            return true;
        } catch (error) {
            console.error('Service worker registration failed:', error);
            return false;
        }
    },

    // Check if running in standalone mode (installed on home screen)
    isStandaloneMode: function () {
        return window.matchMedia('(display-mode: standalone)').matches
            || window.navigator.standalone === true;
    },

    // Check if push notifications are supported
    isPushSupported: function () {
        return 'PushManager' in window
            && 'serviceWorker' in navigator
            && 'Notification' in window;
    },

    // Get current notification permission state
    getPermissionState: function () {
        if (!('Notification' in window)) return 'unsupported';
        return Notification.permission; // 'default', 'granted', 'denied'
    },

    // Check if there is an existing push subscription
    getExistingSubscription: async function () {
        try {
            const reg = await navigator.serviceWorker.ready;
            const subscription = await reg.pushManager.getSubscription();
            if (subscription) {
                this._subscription = subscription;
                return JSON.stringify(subscription.toJSON());
            }
            return null;
        } catch (error) {
            console.error('Error checking subscription:', error);
            return null;
        }
    },

    // Request notification permission and subscribe to push
    // MUST be called from a direct user gesture (click/tap handler)
    requestPermissionAndSubscribe: async function (vapidPublicKey) {
        if (!this.isPushSupported()) {
            return JSON.stringify({ success: false, error: 'Push notifications not supported' });
        }

        try {
            // Request permission - this MUST be inside a user gesture
            const permission = await Notification.requestPermission();

            if (permission !== 'granted') {
                return JSON.stringify({ success: false, error: 'Permission denied', permission: permission });
            }

            // Convert VAPID key from base64url to Uint8Array
            const applicationServerKey = this._urlBase64ToUint8Array(vapidPublicKey);

            // Subscribe to push
            const reg = await navigator.serviceWorker.ready;
            const subscription = await reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerKey
            });

            this._subscription = subscription;

            return JSON.stringify({
                success: true,
                subscription: subscription.toJSON()
            });
        } catch (error) {
            console.error('Push subscription failed:', error);
            return JSON.stringify({ success: false, error: error.message });
        }
    },

    // Unsubscribe from push notifications
    unsubscribe: async function () {
        try {
            const reg = await navigator.serviceWorker.ready;
            const subscription = await reg.pushManager.getSubscription();

            if (subscription) {
                const success = await subscription.unsubscribe();
                if (success) {
                    this._subscription = null;
                    return JSON.stringify({
                        success: true,
                        endpoint: subscription.endpoint
                    });
                }
            }

            return JSON.stringify({ success: false, error: 'No active subscription' });
        } catch (error) {
            console.error('Unsubscribe failed:', error);
            return JSON.stringify({ success: false, error: error.message });
        }
    },

    // Check if subscription is still valid
    isSubscriptionValid: async function () {
        try {
            const reg = await navigator.serviceWorker.ready;
            const subscription = await reg.pushManager.getSubscription();
            return subscription !== null;
        } catch {
            return false;
        }
    },

    // Show a local test notification (for verifying setup)
    showTestNotification: async function () {
        if (Notification.permission !== 'granted') return false;

        try {
            const basePath = this._getBasePath();
            const reg = await navigator.serviceWorker.ready;
            await reg.showNotification('M8L Set Game', {
                body: 'Push notifications are working!',
                icon: basePath + 'icon-192.png',
                badge: basePath + 'icon-192.png',
                tag: 'test',
                data: { url: basePath }
            });
            return true;
        } catch (error) {
            console.error('Test notification failed:', error);
            return false;
        }
    },

    // Helper: Convert base64url VAPID key to Uint8Array
    _urlBase64ToUint8Array: function (base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    },

    // Store subscription in Firebase (via FirebaseInterop)
    saveSubscriptionToFirebase: async function (userId, subscriptionJson) {
        try {
            if (!window.FirebaseInterop || !window.FirebaseInterop.isReady()) {
                console.warn('Firebase not ready');
                return false;
            }

            const db = firebase.database();
            const subscription = JSON.parse(subscriptionJson);

            await db.ref('pushSubscriptions/' + userId).set({
                endpoint: subscription.endpoint,
                keys: {
                    p256dh: subscription.keys.p256dh,
                    auth: subscription.keys.auth
                },
                createdAt: firebase.database.ServerValue.TIMESTAMP,
                userAgent: navigator.userAgent
            });

            return true;
        } catch (error) {
            console.error('Failed to save subscription to Firebase:', error);
            return false;
        }
    },

    // Remove subscription from Firebase
    removeSubscriptionFromFirebase: async function (userId) {
        try {
            if (!window.FirebaseInterop || !window.FirebaseInterop.isReady()) {
                return false;
            }

            const db = firebase.database();
            await db.ref('pushSubscriptions/' + userId).remove();
            return true;
        } catch (error) {
            console.error('Failed to remove subscription from Firebase:', error);
            return false;
        }
    }
};
