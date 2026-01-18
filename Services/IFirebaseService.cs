using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Services
{
    public interface IFirebaseService
    {
        Task<bool> InitializeAsync();
        Task<string> GetUserIdAsync();

        // Solo game operations
        Task<bool> SaveSoloGameAsync(string gameId, Game game);
        Task<Game?> LoadSoloGameAsync(string gameId);

        // Multiplayer room operations
        Task<string> CreateRoomAsync(MultiplayerGame game);
        Task<bool> RoomExistsAsync(string roomCode);
        Task<MultiplayerGame?> GetRoomAsync(string roomCode);
        Task<bool> UpdateRoomAsync(string roomCode, MultiplayerGame game);
        Task<bool> DeleteRoomAsync(string roomCode);

        // Player operations
        Task<bool> AddPlayerToRoomAsync(string roomCode, Player player);
        Task<bool> RemovePlayerFromRoomAsync(string roomCode, string playerId);

        // Real-time subscription
        Task SubscribeToRoomAsync(string roomCode, Action<MultiplayerGame?> onUpdate);
        Task UnsubscribeFromRoomAsync(string roomCode);
    }
}
