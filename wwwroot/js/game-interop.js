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

    // Play sound effect (optional)
    playSound: function (soundType) {
        // Add sound effects if desired
        console.log('Sound:', soundType);
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
