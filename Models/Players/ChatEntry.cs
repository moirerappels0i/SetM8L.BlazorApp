using System.Text.Json.Serialization;
using SetCardGame.BlazorApp.Models.Game;

namespace SetCardGame.BlazorApp.Models.Players
{
    public class ChatEntry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("player")]
        public int Player { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; } = string.Empty;

        [JsonPropertyName("cards")]
        public List<Card> Cards { get; set; } = new List<Card>();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatEntry() { }

        public ChatEntry(string type, int player)
        {
            Type = type;
            Player = player;
            Timestamp = DateTime.Now;
        }

        public static ChatEntry CreateSetEntry(int player, List<Card> cards, string playerName = "")
        {
            return new ChatEntry("set", player)
            {
                Cards = cards,
                PlayerName = playerName
            };
        }

        public static ChatEntry CreateMessageEntry(int player, string message, string playerName = "")
        {
            return new ChatEntry("message", player)
            {
                Message = message,
                PlayerName = playerName
            };
        }
    }
}
