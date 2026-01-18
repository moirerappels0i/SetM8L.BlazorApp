using System.Text.Json.Serialization;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Models.Game
{
    public class MultiplayerGame : Game
    {
        [JsonPropertyName("roomCode")]
        public string RoomCode { get; set; } = string.Empty;

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; } = new List<Player>();

        [JsonPropertyName("isStarted")]
        public bool IsStarted { get; set; } = false;

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [JsonPropertyName("winnerId")]
        public string? WinnerId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("hostPlayerId")]
        public string HostPlayerId { get; set; } = string.Empty;

        public MultiplayerGame() : base() { }

        public MultiplayerGame(string roomCode) : base()
        {
            RoomCode = roomCode;
            CreatedAt = DateTime.Now;
        }

        public MultiplayerGame(string roomCode, string hostPlayerId) : base()
        {
            RoomCode = roomCode;
            HostPlayerId = hostPlayerId;
            CreatedAt = DateTime.Now;
        }

        public Player? GetPlayer(string playerId)
        {
            return Players.FirstOrDefault(p => p.Id == playerId);
        }

        public Player? GetHost()
        {
            return Players.FirstOrDefault(p => p.Id == HostPlayerId);
        }

        public bool IsPlayerHost(string playerId)
        {
            return !string.IsNullOrEmpty(HostPlayerId) && HostPlayerId == playerId;
        }

        public void AddPlayer(Player player)
        {
            var existingPlayer = Players.FirstOrDefault(p => p.Id == player.Id);
            if (existingPlayer != null)
            {
                existingPlayer.Name = player.Name;
            }
            else
            {
                if (string.IsNullOrEmpty(player.Name))
                {
                    player.Name = $"Player {Players.Count + 1}";
                }

                if (!string.IsNullOrEmpty(HostPlayerId) && player.Id == HostPlayerId)
                {
                    Players.Insert(0, player);
                }
                else
                {
                    Players.Add(player);
                }
            }
        }

        public void RemovePlayer(string playerId)
        {
            Players.RemoveAll(p => p.Id == playerId);
        }

        [JsonIgnore]
        public bool AllPlayersReady => Players.Count > 1 && Players.All(p => p.IsReady);

        public Player? GetWinner()
        {
            if (!IsCompleted || !Players.Any()) return null;
            return Players.OrderByDescending(p => p.Score).First();
        }

        public List<Player> GetLeaderboard()
        {
            return Players.OrderByDescending(p => p.Score).ToList();
        }
    }
}
