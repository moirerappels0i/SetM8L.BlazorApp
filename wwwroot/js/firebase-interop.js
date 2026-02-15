// Firebase Configuration
// IMPORTANT: Replace these values with your Firebase project config from Firebase Console
// Get these values from: Firebase Console > Project Settings > Your apps > Web app
// Get databaseURL from: Firebase Console > Realtime Database > Copy the URL at the top
const firebaseConfig = {
  apiKey: "AIzaSyCgyQXeiPDp2aFMgRSaydbYFpwHY-l099c",
  authDomain: "setm8lblazorapp.firebaseapp.com",
  databaseURL: "https://setm8lblazorapp-default-rtdb.europe-west1.firebasedatabase.app",
  projectId: "setm8lblazorapp",
  storageBucket: "setm8lblazorapp.firebasestorage.app",
  messagingSenderId: "499113838509",
  appId: "1:499113838509:web:628021a8ab38a2cb264324"
};

// Initialize Firebase
let firebaseApp = null;
let database = null;
let auth = null;
let currentUserId = null;
let gameListeners = {};
let isInitialized = false;
let connectionRef = null;
let isOnline = true;

// Helper: Convert players array to Firebase object keyed by player ID
function playersArrayToObject(gameData) {
    if (gameData && Array.isArray(gameData.players)) {
        const playersObj = {};
        gameData.players.forEach(function(player) {
            if (player && player.id) {
                playersObj[player.id] = player;
            }
        });
        gameData.players = playersObj;
    }
    return gameData;
}

// Helper: Convert players Firebase object back to array
function playersObjectToArray(gameData) {
    if (gameData && gameData.players && !Array.isArray(gameData.players)) {
        const playersArray = [];
        for (const key in gameData.players) {
            if (gameData.players.hasOwnProperty(key)) {
                const player = gameData.players[key];
                if (player && typeof player === 'object') {
                    if (!player.id) {
                        player.id = key;
                    }
                    playersArray.push(player);
                }
            }
        }
        gameData.players = playersArray;
    }
    return gameData;
}

// Helper: Wrap a promise with a timeout
function withTimeout(promise, ms, errorMsg) {
    return Promise.race([
        promise,
        new Promise(function(_, reject) {
            setTimeout(function() { reject(new Error(errorMsg || 'Operation timed out')); }, ms);
        })
    ]);
}

// Helper: Deep clone and prepare game data for Firebase write
function prepareGameData(gameData) {
    const data = JSON.parse(JSON.stringify(gameData));
    return playersArrayToObject(data);
}

window.FirebaseInterop = {
    // Initialize Firebase with authentication
    initialize: async function (config) {
        try {
            // Prevent double initialization
            if (isInitialized && currentUserId) {
                return { success: true, userId: currentUserId };
            }

            // Handle re-initialization if app already exists
            try {
                if (config && config.apiKey) {
                    firebaseApp = firebase.initializeApp(config);
                } else {
                    if (firebaseConfig.apiKey === "YOUR_API_KEY") {
                        console.error('Firebase config not set! Please update firebase-interop.js with your Firebase project config.');
                        return { success: false, error: 'Firebase configuration not set. Please update firebaseConfig in firebase-interop.js' };
                    }
                    firebaseApp = firebase.initializeApp(firebaseConfig);
                }
            } catch (initError) {
                // App already exists, get the existing one
                if (initError.code === 'app/duplicate-app') {
                    firebaseApp = firebase.app();
                } else {
                    throw initError;
                }
            }

            database = firebase.database();
            auth = firebase.auth();

            // Set auth persistence to local (survives browser restart)
            await auth.setPersistence(firebase.auth.Auth.Persistence.LOCAL);

            // Check for existing auth state first
            const existingUser = auth.currentUser;
            if (existingUser) {
                currentUserId = existingUser.uid;
            } else {
                // Sign in anonymously to get a user ID
                const userCredential = await withTimeout(
                    auth.signInAnonymously(),
                    10000,
                    'Authentication timed out'
                );
                currentUserId = userCredential.user.uid;
            }

            // Set up auth state listener for session persistence
            auth.onAuthStateChanged(function(user) {
                if (user) {
                    currentUserId = user.uid;
                    console.log('Auth state changed. User ID:', currentUserId);
                } else {
                    // Re-authenticate if session lost
                    auth.signInAnonymously().catch(console.error);
                }
            });

            // Monitor connection state
            connectionRef = database.ref('.info/connected');
            connectionRef.on('value', function(snapshot) {
                isOnline = snapshot.val() === true;
                console.log('Connection status:', isOnline ? 'online' : 'offline');

                window.dispatchEvent(new CustomEvent('firebase-connection-changed', {
                    detail: { isOnline: isOnline }
                }));
            });

            isInitialized = true;
            console.log('Firebase initialized successfully. User ID:', currentUserId);
            return { success: true, userId: currentUserId };
        } catch (error) {
            console.error('Firebase initialization error:', error);
            return { success: false, error: error.message };
        }
    },

    // Check if Firebase is initialized
    isReady: function() {
        return isInitialized && currentUserId != null;
    },

    // Check connection status
    isConnected: function() {
        return isOnline;
    },

    // Get current user ID
    getUserId: function () {
        return currentUserId || localStorage.getItem('playerId') || window.FirebaseInterop.generateId();
    },

    // Generate unique ID
    generateId: function () {
        const id = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
        localStorage.setItem('playerId', id);
        return id;
    },

    // Generate room code
    generateRoomCode: function () {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
        let code = '';
        for (let i = 0; i < 6; i++) {
            code += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return code;
    },

    // Save solo game state
    saveSoloGame: async function (gameId, gameData) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const gameRef = database.ref('soloGames/' + gameId);
            await withTimeout(gameRef.set(gameData), 15000, 'Save timed out');
            return { success: true };
        } catch (error) {
            console.error('Error saving solo game:', error);
            return { success: false, error: error.message };
        }
    },

    // Load solo game state
    loadSoloGame: async function (gameId) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const gameRef = database.ref('soloGames/' + gameId);
            const snapshot = await withTimeout(gameRef.get(), 15000, 'Load timed out');
            if (snapshot.exists()) {
                return { success: true, data: snapshot.val() };
            }
            return { success: false, error: 'Game not found' };
        } catch (error) {
            console.error('Error loading solo game:', error);
            return { success: false, error: error.message };
        }
    },

    // Create multiplayer room
    createRoom: async function (roomCode, gameData) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            console.log('Creating room:', roomCode);
            const roomRef = database.ref('rooms/' + roomCode);
            const data = prepareGameData(gameData);
            await withTimeout(roomRef.set(data), 15000, 'Create room timed out');
            console.log('Room created successfully:', roomCode);
            return { success: true, roomCode: roomCode };
        } catch (error) {
            console.error('Error creating room:', error);
            return { success: false, error: error.message };
        }
    },

    // Check if room exists
    roomExists: async function (roomCode) {
        try {
            if (!database) return false;
            const roomRef = database.ref('rooms/' + roomCode);
            const snapshot = await withTimeout(roomRef.get(), 10000, 'Room check timed out');
            return snapshot.exists();
        } catch (error) {
            console.error('Error checking room:', error);
            return false;
        }
    },

    // Get room data
    getRoom: async function (roomCode) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const roomRef = database.ref('rooms/' + roomCode);
            const snapshot = await withTimeout(roomRef.get(), 15000, 'Get room timed out');
            if (snapshot.exists()) {
                const data = playersObjectToArray(snapshot.val());
                return { success: true, data: data };
            }
            return { success: false, error: 'Room not found' };
        } catch (error) {
            console.error('Error getting room:', error);
            return { success: false, error: error.message };
        }
    },

    // Update room data
    updateRoom: async function (roomCode, gameData) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const roomRef = database.ref('rooms/' + roomCode);
            const data = prepareGameData(gameData);
            await withTimeout(roomRef.set(data), 15000, 'Update room timed out');
            return { success: true };
        } catch (error) {
            console.error('Error updating room:', error);
            return { success: false, error: error.message };
        }
    },

    // Add player to room
    addPlayer: async function (roomCode, player) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const playersRef = database.ref('rooms/' + roomCode + '/players/' + player.id);
            await withTimeout(playersRef.set(player), 15000, 'Add player timed out');
            return { success: true };
        } catch (error) {
            console.error('Error adding player:', error);
            return { success: false, error: error.message };
        }
    },

    // Remove player from room
    removePlayer: async function (roomCode, playerId) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const playerRef = database.ref('rooms/' + roomCode + '/players/' + playerId);
            await withTimeout(playerRef.remove(), 15000, 'Remove player timed out');
            return { success: true };
        } catch (error) {
            console.error('Error removing player:', error);
            return { success: false, error: error.message };
        }
    },

    // Subscribe to room updates (real-time)
    subscribeToRoom: function (roomCode, dotNetRef) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const roomRef = database.ref('rooms/' + roomCode);

            // Store listener reference for cleanup
            const listener = roomRef.on('value', function(snapshot) {
                if (snapshot.exists()) {
                    const data = playersObjectToArray(snapshot.val());
                    dotNetRef.invokeMethodAsync('OnRoomUpdated', JSON.stringify(data));
                }
            });

            gameListeners[roomCode] = { ref: roomRef, listener: listener };
            return { success: true };
        } catch (error) {
            console.error('Error subscribing to room:', error);
            return { success: false, error: error.message };
        }
    },

    // Unsubscribe from room updates
    unsubscribeFromRoom: function (roomCode) {
        try {
            if (gameListeners[roomCode]) {
                gameListeners[roomCode].ref.off('value', gameListeners[roomCode].listener);
                delete gameListeners[roomCode];
            }
            return { success: true };
        } catch (error) {
            console.error('Error unsubscribing from room:', error);
            return { success: false, error: error.message };
        }
    },

    // Delete room
    deleteRoom: async function (roomCode) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            window.FirebaseInterop.unsubscribeFromRoom(roomCode);
            const roomRef = database.ref('rooms/' + roomCode);
            await withTimeout(roomRef.remove(), 15000, 'Delete room timed out');
            return { success: true };
        } catch (error) {
            console.error('Error deleting room:', error);
            return { success: false, error: error.message };
        }
    },

    // Get player name from localStorage
    getPlayerName: function () {
        return localStorage.getItem('playerName') || '';
    },

    // Set player name in localStorage
    setPlayerName: function (name) {
        localStorage.setItem('playerName', name);
    },

    // Set up player presence in room (for disconnect detection)
    setupPresence: async function(roomCode, playerId) {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const presenceRef = database.ref('rooms/' + roomCode + '/players/' + playerId + '/online');
            const connectedRef = database.ref('.info/connected');

            connectedRef.on('value', async function(snapshot) {
                if (snapshot.val() === true) {
                    await presenceRef.onDisconnect().set(false);
                    await presenceRef.set(true);
                }
            });

            return { success: true };
        } catch (error) {
            console.error('Error setting up presence:', error);
            return { success: false, error: error.message };
        }
    },

    // Clean up stale rooms (rooms older than 24 hours)
    cleanupStaleRooms: async function() {
        try {
            if (!database) return { success: false, error: 'Firebase not initialized' };
            const roomsRef = database.ref('rooms');
            const snapshot = await withTimeout(roomsRef.get(), 15000, 'Cleanup timed out');

            if (snapshot.exists()) {
                const rooms = snapshot.val();
                const now = Date.now();
                const maxAge = 24 * 60 * 60 * 1000; // 24 hours

                for (const roomCode in rooms) {
                    if (rooms.hasOwnProperty(roomCode)) {
                        const room = rooms[roomCode];
                        if (room.createdAt && (now - room.createdAt) > maxAge) {
                            await database.ref('rooms/' + roomCode).remove();
                            console.log('Cleaned up stale room:', roomCode);
                        }
                    }
                }
            }

            return { success: true };
        } catch (error) {
            console.error('Error cleaning up rooms:', error);
            return { success: false, error: error.message };
        }
    },

    // Validate that user is authenticated before operations
    ensureAuthenticated: async function() {
        if (!auth || !auth.currentUser) {
            try {
                await auth.signInAnonymously();
                return { success: true, userId: auth.currentUser.uid };
            } catch (error) {
                return { success: false, error: 'Authentication required' };
            }
        }
        return { success: true, userId: auth.currentUser.uid };
    },

    // Sign out (clears auth state)
    signOut: async function() {
        try {
            await auth.signOut();
            currentUserId = null;
            return { success: true };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }
};

// Auto-initialize on page load if Firebase SDK is available
if (typeof firebase !== 'undefined') {
    console.log('Firebase SDK detected, ready for initialization');
}

// Cleanup listeners on page unload
window.addEventListener('beforeunload', function() {
    for (const roomCode in gameListeners) {
        if (gameListeners[roomCode]) {
            gameListeners[roomCode].ref.off('value', gameListeners[roomCode].listener);
        }
    }
});
