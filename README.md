# M8L Set Card Game - Blazor WebAssembly
https://moirerappels0i.github.io/SetM8L.BlazorApp/
A client-side Blazor WebAssembly implementation of the Set Card Game with Firebase Realtime Database for persistence and multiplayer support.

## Features

- **Solo Game Mode**: Play alone and find all sets
- **Multiplayer Mode**: Create rooms and play with friends in real-time
- **Theme Customization**: Multiple color and shape themes
- **Assistive Mode**: Visual hints to help find sets
- **PWA Support**: Install as a standalone app
- **Free Hosting**: Can be hosted on GitHub Pages
- **Secure**: Firebase Authentication and Database Security Rules

## Prerequisites

- .NET 8.0 SDK
- A Firebase account (free tier is sufficient)

---

## Quick Start

### 1. Clone and Configure

```bash
git clone <your-repo-url>
cd SetwithM8L
```

### 2. Set Up Firebase (see detailed instructions below)

### 3. Run Locally

```bash
cd BlazorApp
dotnet run
```

### 4. Deploy

Push to `main` branch - GitHub Actions will automatically deploy to GitHub Pages!

---

## Firebase Setup (Detailed)

### Step 1: Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click **"Add project"** and follow the setup wizard
3. Disable Google Analytics (optional, not needed for this app)
4. Click **"Create project"**

### Step 2: Enable Realtime Database

1. In your Firebase project, go to **"Build"** > **"Realtime Database"**
2. Click **"Create Database"**
3. Choose a location close to your users (e.g., `us-central1`)
4. Select **"Start in test mode"** (we'll secure it in Step 5)
5. Click **"Enable"**

### Step 3: Enable Anonymous Authentication

1. Go to **"Build"** > **"Authentication"**
2. Click **"Get started"**
3. Go to **"Sign-in method"** tab
4. Click on **"Anonymous"**
5. Toggle **"Enable"** and click **"Save"**

### Step 4: Get Your Firebase Configuration

1. Go to **Project Settings** (gear icon next to "Project Overview")
2. Scroll down to **"Your apps"**
3. Click the web icon **"</>"**
4. Register your app with a nickname (e.g., "Set Card Game")
5. Copy the `firebaseConfig` object

### Step 5: Update the Configuration

Open `BlazorApp/wwwroot/js/firebase-interop.js` and replace the placeholder config:

```javascript
const firebaseConfig = {
    apiKey: "AIzaSyB...",                                                   // Your API key
    authDomain: "your-project-id.firebaseapp.com",                          // Your auth domain
    databaseURL: "https://your-project-id-default-rtdb.firebaseio.com",     // Your database URL (REQUIRED!)
    projectId: "your-project-id",                                           // Your project ID
    storageBucket: "your-project-id.appspot.com",                           // Your storage bucket
    messagingSenderId: "123456789",                                         // Your sender ID
    appId: "1:123456789:web:abc123def456"                                   // Your app ID
};
```

> **IMPORTANT**: The `databaseURL` field is required for Realtime Database to work. Find it in Firebase Console > Realtime Database (the URL shown at the top of the page).
```

### Step 6: Apply Security Rules (IMPORTANT!)

Go to **Realtime Database** > **"Rules"** tab and replace with:

```json
{
  "rules": {
    "rooms": {
      "$roomCode": {
        ".read": "auth != null",
        ".write": "auth != null"
      }
    },

    "soloGames": {
      "$userId": {
        ".read": "auth != null && auth.uid == $userId",
        ".write": "auth != null && auth.uid == $userId"
      }
    },

    "players": {
      "$userId": {
        ".read": "auth != null && auth.uid == $userId",
        ".write": "auth != null && auth.uid == $userId"
      }
    },

    ".read": false,
    ".write": false
  }
}
```

Click **"Publish"** to apply the rules.

#### What These Rules Do:

| Rule | Purpose |
|------|---------|
| `auth != null` | Requires anonymous authentication (prevents unauthenticated access) |
| `$roomCode` validation | Ensures rooms have required fields |
| Name length validation | Prevents XSS via extremely long names (1-20 chars) |
| Message length validation | Limits chat messages to 500 characters |
| Solo games isolation | Users can only access their own saved games |
| Default deny | All other paths are blocked |

---

## GitHub Pages Deployment

### Automatic Deployment (GitHub Actions)

The repository includes a GitHub Actions workflow (`.github/workflows/deploy.yml`) that automatically:

1. Builds the Blazor WebAssembly app
2. Configures the base URL for GitHub Pages
3. Deploys to GitHub Pages

**To enable:**

1. Go to your repository **Settings** > **Pages**
2. Under "Build and deployment", set Source to **"GitHub Actions"**
3. Push to `main` or `master` branch
4. The app will be deployed to `https://<username>.github.io/<repo-name>/`

### Manual Deployment

```bash
cd BlazorApp
dotnet publish -c Release -o publish

# The published files are in publish/wwwroot
# Copy these to your gh-pages branch or hosting provider
```

---

## Running Locally

```bash
cd BlazorApp
dotnet restore
dotnet run
```

The app will be available at:
- `https://localhost:5001` (HTTPS)
- `http://localhost:5000` (HTTP)

---

## Project Structure

```
SetwithM8L/
├── .github/
│   └── workflows/
│       └── deploy.yml          # GitHub Actions deployment
├── BlazorApp/
│   ├── Models/
│   │   ├── Game/               # Card, Game, MultiplayerGame, ThemeConfiguration
│   │   ├── Players/            # Player, ChatEntry
│   │   └── ViewModels/         # GameViewModel, MultiplayerGameViewModel
│   ├── Services/
│   │   ├── FirebaseService.cs  # Firebase integration
│   │   ├── GameStateService.cs # Game logic orchestration
│   │   ├── SharedGameService.cs# Set validation, deck generation
│   │   ├── ThemeService.cs     # Theme management
│   │   └── PlayerService.cs    # Player identity management
│   ├── Pages/
│   │   ├── Index.razor         # Home page
│   │   ├── SoloGame.razor      # Solo game mode
│   │   ├── Multiplayer.razor   # Create/join multiplayer
│   │   ├── WaitingRoom.razor   # Multiplayer lobby
│   │   └── MultiplayerGame.razor# Multiplayer game
│   ├── Shared/
│   │   ├── MainLayout.razor    # App layout with navigation
│   │   ├── CardDisplay.razor   # Card rendering component
│   │   ├── ChatSection.razor   # Chat/game log component
│   │   └── *.razor             # Other shared components
│   └── wwwroot/
│       ├── css/game.css        # All styling
│       ├── js/
│       │   ├── firebase-interop.js  # Firebase JavaScript SDK integration
│       │   └── game-interop.js      # General JS interop utilities
│       ├── index.html          # Main HTML entry point
│       └── manifest.json       # PWA manifest
├── database.rules.json         # Firebase security rules (reference)
└── firebase.json               # Firebase configuration
```

---

## Security Features

### Authentication
- **Anonymous Authentication**: Each browser session gets a unique, authenticated user ID
- **Session Persistence**: Auth state survives browser restarts
- **Auto Re-authentication**: Automatically re-authenticates if session is lost

### Database Security
- **Authenticated Access Only**: All database operations require authentication
- **Data Validation**: Server-side validation of all data structures
- **Input Length Limits**: Prevents abuse via extremely large inputs
- **User Isolation**: Solo game data is private to each user
- **Room Validation**: Multiplayer rooms must have required structure

### Client-Side Security
- **Connection Monitoring**: Detects online/offline status
- **Presence Detection**: Tracks player connection status in multiplayer
- **Automatic Cleanup**: Stale rooms are automatically removed
- **Listener Cleanup**: Properly cleans up Firebase listeners on page unload

---

## How It Works

### Solo Mode
- Game state runs entirely in the browser
- Optionally saves to Firebase (authenticated, private to user)
- Timer runs locally

### Multiplayer Mode
1. **Create Room**: Host generates a 6-character room code
2. **Join Room**: Players join using the room code
3. **Waiting Room**: Host sees all players, can start when ready
4. **Real-time Sync**: All game state synced via Firebase Realtime Database
5. **Set Validation**: When any player finds a set, all players see it instantly
6. **Leaderboard**: Live score updates for all players

---

## Troubleshooting

### "Firebase configuration not set"
- Update `firebaseConfig` in `wwwroot/js/firebase-interop.js` with your Firebase project values

### "Permission denied" errors
- Ensure Anonymous Authentication is enabled in Firebase Console
- Check that security rules are published correctly

### App not loading on GitHub Pages
- Check that GitHub Pages is configured to use GitHub Actions
- Verify the base href is correct in the published `index.html`

### Multiplayer not syncing
- Check browser console for Firebase errors
- Verify your `databaseURL` is correct in the config

---

## License

MIT License
