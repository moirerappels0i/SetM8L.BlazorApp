# CLAUDE.md — AI Assistant Guide for SetM8L.BlazorApp

## Project Overview

SetM8L.BlazorApp is a **Blazor WebAssembly** implementation of the Set card game. It supports solo play and real-time multiplayer via Firebase Realtime Database. The app is entirely client-side (no server) and deploys to GitHub Pages.

**Tech stack**: .NET 8.0, Blazor WebAssembly, C# 12, Firebase Realtime Database, vanilla CSS, JavaScript interop.

## Quick Reference

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run locally (http://localhost:12826)
dotnet run

# Publish release
dotnet publish -c Release
```

There are **no tests** in this project. No test framework is configured.

## Repository Structure

```
SetM8L.BlazorApp/
├── Models/
│   ├── Game/            # Card, Game, MultiplayerGame, ThemeConfiguration
│   ├── Players/         # Player, ChatEntry
│   └── ViewModels/      # GameViewModel, MultiplayerGameViewModel, WaitingRoomViewModel
├── Pages/               # Routable Blazor pages (Index, SoloGame, Multiplayer, etc.)
├── Services/            # Business logic (interfaces + implementations)
├── Shared/              # Reusable components (CardDisplay, ChatSection, popups, MainLayout)
├── Properties/          # launchSettings.json
├── wwwroot/
│   ├── index.html       # SPA entry point
│   ├── css/game.css     # All styling (~2400 lines)
│   ├── js/
│   │   ├── firebase-interop.js   # Firebase SDK wrapper (374 lines)
│   │   └── game-interop.js       # Game UI utilities (149 lines)
│   └── manifest.json    # PWA manifest
├── Program.cs           # Entry point, DI registration
├── App.razor            # Router configuration
├── _Imports.razor        # Global using statements
├── SetCardGame.BlazorApp.csproj
├── SetCardGame.BlazorApp.sln
├── firebase.json        # Firebase hosting config
└── database.rules.json  # Firebase security rules
```

## Architecture

### Service Layer

All services are registered as **scoped** in `Program.cs` and follow the interface/implementation pattern:

| Service | Interface | Responsibility |
|---------|-----------|---------------|
| `SharedGameService` | `ISharedGameService` | Deck generation, set validation, hint finding |
| `GameStateService` | `IGameStateService` | Game orchestration, viewmodel creation, scoring |
| `FirebaseService` | `IFirebaseService` | Firebase CRUD, real-time subscriptions, auth |
| `PlayerService` | `IPlayerService` | Player identity, localStorage persistence |
| `ThemeService` | `IThemeService` | Theme management |

### Data Flow

```
Razor Components → GameStateService → SharedGameService (logic)
                                    → FirebaseService (persistence)
                                    → PlayerService (identity)
```

### Pages and Routing

| Page | Route | Purpose |
|------|-------|---------|
| `Index.razor` | `/` | Home / landing |
| `SoloGame.razor` | `/solo` | Solo game mode |
| `Multiplayer.razor` | `/multiplayer` | Create/join multiplayer room |
| `JoinRoom.razor` | `/join/{RoomCode}` | Room join confirmation |
| `WaitingRoom.razor` | `/waiting/{RoomCode}` | Multiplayer lobby |
| `PlayGame.razor` | `/play/{RoomCode}` | Active multiplayer game |

### JavaScript Interop

The app uses `IJSRuntime.InvokeAsync<T>()` to call into two JS modules:

- **firebase-interop.js** — Firebase SDK initialization, anonymous auth, room CRUD, real-time listeners, event callbacks to C#
- **game-interop.js** — Game UI utilities and interaction handlers

### Firebase Data Structure

```
/rooms/{roomCode}/       — Multiplayer rooms (roomCode, hostId, players[], visibleCards[], chatLogEntries[])
/soloGames/{userId}/     — Solo game saves (score, visibleCards[])
/players/{userId}/       — Player profiles (name, lastSeen)
```

## Game Logic

The Set card game uses cards with 4 attributes (Shape, Color, Fill, Number), each with 3 possible values, producing an 81-card deck. A valid "set" is 3 cards where each attribute is either all the same or all different across the three cards.

Key algorithms in `SharedGameService`:
- `GenerateDeck()` — Creates 81 cards from theme configuration
- `IsValidSet()` — Validates a 3-card set
- `FindAllSets()` — Brute-force O(n^3) search for all valid sets on the board
- `EnsurePlayableBoard()` — Maintains at least 12 visible cards with at least one valid set

## Coding Conventions

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `SharedGameService` |
| Interfaces | `I` + PascalCase | `ISharedGameService` |
| Methods | PascalCase, verb-first | `GenerateDeck()`, `IsValidSet()` |
| Private fields | `_camelCase` | `_jsRuntime`, `_isInitialized` |
| Namespaces | `SetCardGame.BlazorApp.*` | `SetCardGame.BlazorApp.Services` |
| Razor pages | PascalCase | `SoloGame.razor` |
| Routes | lowercase/kebab | `/solo`, `/multiplayer` |
| CSS classes | kebab-case | `game-header`, `main-container` |
| JSON properties | camelCase via `[JsonPropertyName]` | `roomCode`, `hostId` |

### Patterns

- **Interface segregation**: Small, focused interfaces (e.g., `IPlayerService` has only 3 methods)
- **Async/await**: All Firebase and JS interop calls are async
- **ViewModel pattern**: Domain models are transformed into view models for UI binding
- **Scoped DI**: All services are scoped (one instance per Blazor circuit)
- **No code-behind**: Razor components use `@code` blocks inline

### File Organization

- One interface and one implementation per service, both in `Services/`
- Models are grouped by domain: `Game/`, `Players/`, `ViewModels/`
- Shared/reusable components go in `Shared/`
- Each routable page has its own file in `Pages/`

## Deployment

CI/CD is configured via `.github/workflows/deploy.yml`:

1. Triggers on push to `main`/`master`, PRs, or manual dispatch
2. Builds with `dotnet publish -c Release`
3. Adjusts base href for GitHub Pages
4. Deploys to GitHub Pages

The app runs entirely client-side — no server needed.

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Components.WebAssembly` 8.0.0
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer` 8.0.0
- `System.Net.Http.Json` 8.0.0

### External (loaded via CDN in index.html)
- Firebase SDK v10.7.0 (Auth + Realtime Database)

## Security

- Firebase Anonymous Authentication for user identity
- Firebase security rules enforce auth, data validation, and input length limits
- Solo game data is isolated per user
- Room-level access control for multiplayer

## Key Considerations for AI Assistants

- This is a **single-project solution** — no separate API, no test project
- All persistence goes through **Firebase via JS interop**, not .NET HTTP clients
- The CSS is in a single large file (`wwwroot/css/game.css`, ~2400 lines) — not component-scoped
- There are **no unit tests** — consider the impact of changes carefully
- The project targets **.NET 8.0** — do not use .NET 9+ APIs
- Blazor WebAssembly runs in the browser — no server-side code, no direct database access
- When adding new services, register them as scoped in `Program.cs` and create an interface
- When adding new pages, use the `@page` directive with a route and place the file in `Pages/`
