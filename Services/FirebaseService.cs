using Microsoft.JSInterop;
using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;
using System.Text.Json;

namespace SetCardGame.BlazorApp.Services
{
    public class FirebaseService : IFirebaseService, IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized = false;
        private string? _userId;
        private DotNetObjectReference<FirebaseService>? _dotNetRef;
        private Action<MultiplayerGame?>? _roomUpdateCallback;

        public FirebaseService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;

            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.initialize", (object?)null);
                _isInitialized = result.GetProperty("success").GetBoolean();

                if (_isInitialized && result.TryGetProperty("userId", out var userIdProp))
                {
                    _userId = userIdProp.GetString();
                }

                return _isInitialized;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firebase initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetUserIdAsync()
        {
            if (!string.IsNullOrEmpty(_userId))
                return _userId;

            try
            {
                _userId = await _jsRuntime.InvokeAsync<string>("FirebaseInterop.getUserId");
                return _userId ?? Guid.NewGuid().ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        public async Task<bool> SaveSoloGameAsync(string gameId, Game game)
        {
            try
            {
                var gameJson = JsonSerializer.Serialize(game);
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.saveSoloGame", gameId, game);
                return result.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving solo game: {ex.Message}");
                return false;
            }
        }

        public async Task<Game?> LoadSoloGameAsync(string gameId)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.loadSoloGame", gameId);
                if (result.GetProperty("success").GetBoolean())
                {
                    var data = result.GetProperty("data");
                    return JsonSerializer.Deserialize<Game>(data.GetRawText());
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading solo game: {ex.Message}");
                return null;
            }
        }

        public async Task<string> CreateRoomAsync(MultiplayerGame game)
        {
            try
            {
                var roomCode = await _jsRuntime.InvokeAsync<string>("FirebaseInterop.generateRoomCode");

                // Ensure room code is unique
                while (await RoomExistsAsync(roomCode))
                {
                    roomCode = await _jsRuntime.InvokeAsync<string>("FirebaseInterop.generateRoomCode");
                }

                game.RoomCode = roomCode;

                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.createRoom", roomCode, game);
                if (result.GetProperty("success").GetBoolean())
                {
                    return roomCode;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating room: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> RoomExistsAsync(string roomCode)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("FirebaseInterop.roomExists", roomCode);
            }
            catch
            {
                return false;
            }
        }

        public async Task<MultiplayerGame?> GetRoomAsync(string roomCode)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.getRoom", roomCode);
                if (result.GetProperty("success").GetBoolean())
                {
                    var data = result.GetProperty("data");
                    return DeserializeMultiplayerGame(data.GetRawText());
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting room: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateRoomAsync(string roomCode, MultiplayerGame game)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.updateRoom", roomCode, game);
                return result.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating room: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(string roomCode)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.deleteRoom", roomCode);
                return result.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting room: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddPlayerToRoomAsync(string roomCode, Player player)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.addPlayer", roomCode, player);
                return result.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding player: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemovePlayerFromRoomAsync(string roomCode, string playerId)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<JsonElement>("FirebaseInterop.removePlayer", roomCode, playerId);
                return result.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing player: {ex.Message}");
                return false;
            }
        }

        public async Task SubscribeToRoomAsync(string roomCode, Action<MultiplayerGame?> onUpdate)
        {
            try
            {
                _roomUpdateCallback = onUpdate;
                _dotNetRef = DotNetObjectReference.Create(this);

                await _jsRuntime.InvokeVoidAsync("FirebaseInterop.subscribeToRoom", roomCode, _dotNetRef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to room: {ex.Message}");
            }
        }

        [JSInvokable]
        public void OnRoomUpdated(string jsonData)
        {
            try
            {
                var game = DeserializeMultiplayerGame(jsonData);
                _roomUpdateCallback?.Invoke(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing room update: {ex.Message}");
            }
        }

        public async Task UnsubscribeFromRoomAsync(string roomCode)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("FirebaseInterop.unsubscribeFromRoom", roomCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unsubscribing from room: {ex.Message}");
            }
        }

        private MultiplayerGame? DeserializeMultiplayerGame(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<MultiplayerGame>(json, options);
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _dotNetRef?.Dispose();
        }
    }
}
