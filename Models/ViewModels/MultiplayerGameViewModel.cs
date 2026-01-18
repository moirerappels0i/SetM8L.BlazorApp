using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Models.ViewModels
{
    public class MultiplayerGameViewModel : GameViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
        public string CurrentPlayerId { get; set; } = string.Empty;
        public bool IsGameStarted { get; set; }
        public bool IsGameCompleted { get; set; }
        public Player? Winner { get; set; }
        public List<Player> Leaderboard { get; set; } = new List<Player>();
        public string WinnerDisplayName { get; set; } = string.Empty;
        public bool IsHost { get; set; }
    }
}
