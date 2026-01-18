using System.Text.Json.Serialization;

namespace SetCardGame.BlazorApp.Models.Players
{
    public class Player
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("isReady")]
        public bool IsReady { get; set; }

        [JsonPropertyName("joinedAt")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        public Player() { }

        public Player(string id, string name)
        {
            Id = id;
            Name = name;
            Score = 0;
            IsReady = false;
            JoinedAt = DateTime.Now;
        }
    }
}
