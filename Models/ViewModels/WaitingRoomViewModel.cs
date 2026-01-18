using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Models.ViewModels
{
    public class WaitingRoomViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string InviteUrl { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
        public bool IsHost { get; set; }
        public string CurrentPlayerName { get; set; } = string.Empty;
        public string CurrentPlayerId { get; set; } = string.Empty;
        public bool CanStartGame => Players.Count >= 2 && IsHost;
    }
}
