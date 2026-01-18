using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Models.ViewModels
{
    public class GameViewModel
    {
        public List<Card> VisibleCards { get; set; } = new List<Card>();
        public List<ChatEntry> ChatEntries { get; set; } = new List<ChatEntry>();
        public int CurrentPlayerIndex { get; set; } = 1;
        public TimeSpan ElapsedTime { get; set; }
        public int RemainingCards { get; set; }
        public int AvailableSets { get; set; }
        public bool IsGameComplete { get; set; }
        public ThemeConfiguration Theme { get; set; } = new ThemeConfiguration();
        public string GameId { get; set; } = string.Empty;
        public string GameEndMessage { get; set; } = string.Empty;
        public string CurrentPlayerName { get; set; } = string.Empty;
        public int Score { get; set; }

        public string FormattedTime => $"{(int)ElapsedTime.TotalMinutes}:{ElapsedTime.Seconds:D2}";

        public string SetStatusMessage => AvailableSets > 0
            ? $"Status: {AvailableSets} set{(AvailableSets > 1 ? "s" : "")} can be found."
            : "Status: No sets can be found.";
    }
}
