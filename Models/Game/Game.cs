using System.Text.Json.Serialization;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Models.Game
{
    public class Game
    {
        [JsonPropertyName("gameId")]
        public string GameId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("deck")]
        public List<Card> Deck { get; set; } = new List<Card>();

        [JsonPropertyName("visibleCards")]
        public List<Card> VisibleCards { get; set; } = new List<Card>();

        [JsonPropertyName("selectedCards")]
        public List<Card> SelectedCards { get; set; } = new List<Card>();

        [JsonPropertyName("chatLogEntries")]
        public List<ChatEntry> ChatLogEntries { get; set; } = new List<ChatEntry>();

        [JsonPropertyName("currentPlayerIndex")]
        public int CurrentPlayerIndex { get; set; } = 1;

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [JsonPropertyName("theme")]
        public ThemeConfiguration Theme { get; set; } = new ThemeConfiguration();

        [JsonPropertyName("nextCardIndex")]
        public int NextCardIndex { get; set; } = 0;

        [JsonPropertyName("score")]
        public int Score { get; set; } = 0;

        [JsonIgnore]
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;

        [JsonIgnore]
        public int RemainingCards => Deck.Count - NextCardIndex;

        public Card? DealNextCard()
        {
            if (NextCardIndex >= Deck.Count)
                return null;

            return Deck[NextCardIndex++];
        }

        public List<Card> DealCards(int count)
        {
            var cards = new List<Card>();
            for (int i = 0; i < count && NextCardIndex < Deck.Count; i++)
            {
                cards.Add(Deck[NextCardIndex++]);
            }
            return cards;
        }
    }
}
