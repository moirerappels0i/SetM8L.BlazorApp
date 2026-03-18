// Game UI Interop for Blazor
window.GameInterop = {
    // Show notification
    showNotification: function (message, type, duration) {
        const notification = document.createElement('div');
        notification.className = 'notification';
        notification.style.cssText = `
            position: fixed;
            top: 2rem;
            left: 50%;
            transform: translateX(-50%);
            padding: 1rem 1.5rem;
            border-radius: 0.75rem;
            font-weight: 600;
            z-index: 1001;
            box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1);
            animation: slideDownNotification 0.5s ease;
            min-width: 200px;
            text-align: center;
        `;

        switch (type) {
            case 'success':
                notification.style.background = 'linear-gradient(135deg, #10b981, #059669)';
                notification.style.color = 'white';
                break;
            case 'error':
                notification.style.background = 'linear-gradient(135deg, #ef4444, #dc2626)';
                notification.style.color = 'white';
                break;
            case 'warning':
                notification.style.background = 'linear-gradient(135deg, #f59e0b, #d97706)';
                notification.style.color = 'white';
                break;
            default:
                notification.style.background = 'linear-gradient(135deg, #2563eb, #1d4ed8)';
                notification.style.color = 'white';
        }

        notification.textContent = message;
        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateX(-50%) translateY(-20px)';
            notification.style.transition = 'all 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, duration || 3000);
    },

    // Copy text to clipboard
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy:', err);
            return false;
        }
    },

    // Get current URL
    getCurrentUrl: function () {
        return window.location.href;
    },

    // Get base URL
    getBaseUrl: function () {
        return window.location.origin;
    },

    // Scroll chat to bottom
    scrollChatToBottom: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    },

    // Focus input element
    focusElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },

    // Play sound effect using Web Audio API
    playSound: function (soundType) {
        try {
            var ctx = new (window.AudioContext || window.webkitAudioContext)();
            var oscillator = ctx.createOscillator();
            var gain = ctx.createGain();
            oscillator.connect(gain);
            gain.connect(ctx.destination);

            switch (soundType) {
                case 'notification-on':
                    // Two-tone ascending chime
                    oscillator.type = 'sine';
                    oscillator.frequency.setValueAtTime(523.25, ctx.currentTime);       // C5
                    oscillator.frequency.setValueAtTime(659.25, ctx.currentTime + 0.12); // E5
                    oscillator.frequency.setValueAtTime(783.99, ctx.currentTime + 0.24); // G5
                    gain.gain.setValueAtTime(0.3, ctx.currentTime);
                    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.45);
                    oscillator.start(ctx.currentTime);
                    oscillator.stop(ctx.currentTime + 0.45);
                    break;
                case 'notification':
                    // Single bell tone
                    oscillator.type = 'sine';
                    oscillator.frequency.setValueAtTime(830, ctx.currentTime);
                    gain.gain.setValueAtTime(0.25, ctx.currentTime);
                    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.3);
                    oscillator.start(ctx.currentTime);
                    oscillator.stop(ctx.currentTime + 0.3);
                    break;
                default:
                    oscillator.type = 'sine';
                    oscillator.frequency.setValueAtTime(600, ctx.currentTime);
                    gain.gain.setValueAtTime(0.2, ctx.currentTime);
                    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.2);
                    oscillator.start(ctx.currentTime);
                    oscillator.stop(ctx.currentTime + 0.2);
            }
        } catch (err) {
            console.log('Sound not available:', err);
        }
    },

    // Vibrate (mobile)
    vibrate: function (pattern) {
        if (navigator.vibrate) {
            navigator.vibrate(pattern);
        }
    },

    // Set page title
    setTitle: function (title) {
        document.title = title + ' - M8L Set GAME';
    },

    // Local storage helpers
    storage: {
        get: function (key) {
            return localStorage.getItem(key);
        },
        set: function (key, value) {
            localStorage.setItem(key, value);
        },
        remove: function (key) {
            localStorage.removeItem(key);
        }
    },

    // Notification helpers
    notifications: {
        // Check if browser notifications are supported
        isSupported: function () {
            return 'Notification' in window;
        },

        // Check if running as standalone PWA
        isStandalone: function () {
            return window.matchMedia('(display-mode: standalone)').matches
                || window.navigator.standalone === true;
        },

        // Get current permission status: 'granted', 'denied', 'default', or 'unsupported'
        getPermission: function () {
            if (!('Notification' in window)) return 'unsupported';
            return Notification.permission;
        },

        // Request notification permission (must be called from user gesture)
        requestPermission: async function () {
            if (!('Notification' in window)) return 'unsupported';
            try {
                const result = await Notification.requestPermission();
                return result;
            } catch (err) {
                console.error('Notification permission request failed:', err);
                return 'denied';
            }
        },

        // Show a browser notification
        show: function (title, body, icon, url) {
            if (!('Notification' in window) || Notification.permission !== 'granted') {
                return false;
            }

            // Don't show if the page is visible and focused
            if (document.visibilityState === 'visible' && document.hasFocus()) {
                return false;
            }

            try {
                // Play notification sound
                GameInterop.playSound('notification');

                const options = {
                    body: body || '',
                    icon: icon || 'icon-192.png',
                    badge: 'icon-192.png',
                    data: { url: url || '/' },
                    tag: 'setm8l-' + Date.now(),
                    requireInteraction: false
                };

                // Try service worker notification first (works better in standalone PWA)
                if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
                    navigator.serviceWorker.ready.then(function (reg) {
                        reg.showNotification(title, options);
                    });
                } else {
                    var notif = new Notification(title, options);
                    notif.onclick = function () {
                        window.focus();
                        if (url) {
                            var fullUrl = window.location.origin + window.location.pathname.replace(/\/[^/]*$/, '/') + url;
                            window.location.href = fullUrl;
                        }
                        notif.close();
                    };
                }
                return true;
            } catch (err) {
                console.error('Failed to show notification:', err);
                return false;
            }
        },

        // Show a test notification with sound (always shows, even when focused)
        showTest: function (title, body) {
            if (!('Notification' in window) || Notification.permission !== 'granted') {
                return false;
            }

            try {
                // Play the notification-on sound
                GameInterop.playSound('notification-on');

                var options = {
                    body: body || '',
                    icon: 'icon-192.png',
                    badge: 'icon-192.png',
                    tag: 'setm8l-test',
                    requireInteraction: false
                };

                // Try service worker notification first
                if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
                    navigator.serviceWorker.ready.then(function (reg) {
                        reg.showNotification(title, options);
                    });
                } else {
                    new Notification(title, options);
                }
                return true;
            } catch (err) {
                console.error('Failed to show test notification:', err);
                return false;
            }
        },

        // Check if notifications are enabled (permission granted + user preference)
        isEnabled: function () {
            if (!('Notification' in window)) return false;
            var pref = localStorage.getItem('notifications-enabled');
            return Notification.permission === 'granted' && pref === 'true';
        },

        // Set user preference for notifications
        setEnabled: function (enabled) {
            localStorage.setItem('notifications-enabled', enabled ? 'true' : 'false');
        },

        // Get user preference
        getEnabled: function () {
            return localStorage.getItem('notifications-enabled') === 'true';
        },

        // In-app notification preference (toasts while on the page)
        isInAppEnabled: function () {
            var pref = localStorage.getItem('notifications-inapp-enabled');
            // Default to true if not set
            return pref === null ? true : pref === 'true';
        },

        setInAppEnabled: function (enabled) {
            localStorage.setItem('notifications-inapp-enabled', enabled ? 'true' : 'false');
        }
    },

    // Visibility tracking for player status
    setupVisibilityTracking: function (dotNetRef) {
        window._setm8l_visibilityRef = dotNetRef;
        window._setm8l_visibilityHandler = function () {
            if (window._setm8l_visibilityRef) {
                window._setm8l_visibilityRef.invokeMethodAsync('OnVisibilityChanged', !document.hidden);
            }
        };
        document.addEventListener('visibilitychange', window._setm8l_visibilityHandler);
    },

    cleanupVisibilityTracking: function () {
        if (window._setm8l_visibilityHandler) {
            document.removeEventListener('visibilitychange', window._setm8l_visibilityHandler);
            delete window._setm8l_visibilityHandler;
            delete window._setm8l_visibilityRef;
        }
    },

    // Timer helpers
    timers: {},

    startTimer: function (timerId, callback, interval) {
        this.stopTimer(timerId);
        this.timers[timerId] = setInterval(callback, interval);
    },

    stopTimer: function (timerId) {
        if (this.timers[timerId]) {
            clearInterval(this.timers[timerId]);
            delete this.timers[timerId];
        }
    }
};

// Add CSS animation for notifications
const style = document.createElement('style');
style.textContent = `
    @keyframes slideDownNotification {
        from {
            transform: translateX(-50%) translateY(-100%);
            opacity: 0;
        }
        to {
            transform: translateX(-50%) translateY(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);
