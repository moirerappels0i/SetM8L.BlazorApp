using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;
using SetCardGame.BlazorApp.Models.ViewModels;

namespace SetCardGame.BlazorApp.Services
{
    public class GameStateService : IGameStateService
    {
        private readonly ISharedGameService _sharedGameService;
        private readonly IThemeService _themeService;

        public GameStateService(ISharedGameService sharedGameService, IThemeService themeService)
        {
            _sharedGameService = sharedGameService;
            _themeService = themeService;
        }

        public Game CreateNewSoloGame(ThemeConfiguration? existingTheme = null)
        {
            var game = new Game();

            if (existingTheme != null)
            {
                game.Theme.ColorThemeIndex = existingTheme.ColorThemeIndex;
                game.Theme.ShapeThemeIndex = existingTheme.ShapeThemeIndex;
            }
            else
            {
                game.Theme = _themeService.GetDefaultTheme();
            }

            game.Deck = _sharedGameService.GenerateDeck(game.Theme);
            _sharedGameService.ShuffleDeck(game.Deck);
            _sharedGameService.EnsurePlayableBoard(game);

            return game;
        }

        public GameViewModel GetSoloGameViewModel(Game game, string playerName)
        {
            var allSets = _sharedGameService.FindAllSets(game.VisibleCards);
            var isGameComplete = game.RemainingCards == 0 && allSets.Count == 0;

            return new GameViewModel
            {
                VisibleCards = game.VisibleCards,
                ChatEntries = game.ChatLogEntries.OrderByDescending(e => e.Timestamp).ToList(),
                CurrentPlayerIndex = game.CurrentPlayerIndex,
                ElapsedTime = game.ElapsedTime,
                RemainingCards = game.RemainingCards,
                AvailableSets = allSets.Count,
                IsGameComplete = isGameComplete,
                GameEndMessage = isGameComplete ? "No more sets available! Game completed!" : string.Empty,
                Theme = game.Theme,
                GameId = game.GameId,
                CurrentPlayerName = playerName ?? "Player",
                Score = game.Score
            };
        }

        public (bool IsValid, string Message, List<Card> ValidatedCards) ValidateSet(Game game, List<int> cardIndices, string playerName)
        {
            if (cardIndices.Count != 3)
            {
                return (false, "Must select exactly 3 cards", new List<Card>());
            }

            if (cardIndices.Any(i => i < 0 || i >= game.VisibleCards.Count))
            {
                return (false, "Invalid card selection", new List<Card>());
            }

            var selectedCards = cardIndices.Select(i => game.VisibleCards[i]).ToList();
            var isValid = _sharedGameService.IsValidSet(selectedCards);

            if (isValid)
            {
                var actualPlayerName = !string.IsNullOrWhiteSpace(playerName) ? playerName : $"Player {game.CurrentPlayerIndex}";
                var chatEntry = ChatEntry.CreateSetEntry(game.CurrentPlayerIndex, selectedCards, actualPlayerName);
                game.ChatLogEntries.Add(chatEntry);

                game.Score++;

                var indicesToRemove = cardIndices.OrderByDescending(i => i).ToList();
                foreach (var index in indicesToRemove)
                {
                    game.VisibleCards.RemoveAt(index);
                }

                _sharedGameService.EnsurePlayableBoard(game);

                return (true, "Valid set found!", selectedCards);
            }

            return (false, "Not a valid set", selectedCards);
        }

        public void ChangeTheme(Game game, string themeType, int themeIndex)
        {
            var oldColors = new List<string>(game.Theme.CurrentColors);
            var oldShapes = new List<string>(game.Theme.CurrentShapes);

            _themeService.ChangeTheme(game.Theme, themeType, themeIndex);

            var newColors = game.Theme.CurrentColors;
            var newShapes = game.Theme.CurrentShapes;

            if (themeType == "color")
            {
                _sharedGameService.UpdateCardColors(game.VisibleCards, oldColors, newColors);
                _sharedGameService.UpdateCardColors(game.Deck, oldColors, newColors);
                _sharedGameService.UpdateChatEntryColors(game.ChatLogEntries, oldColors, newColors);
            }
            else if (themeType == "shape")
            {
                _sharedGameService.UpdateCardShapes(game.VisibleCards, oldShapes, newShapes);
                _sharedGameService.UpdateCardShapes(game.Deck, oldShapes, newShapes);
                _sharedGameService.UpdateChatEntryShapes(game.ChatLogEntries, oldShapes, newShapes);
            }
        }

        public MultiplayerGame CreateNewMultiplayerGame(string hostId, string hostName)
        {
            var game = new MultiplayerGame(string.Empty, hostId);
            game.Theme = _themeService.GetDefaultTheme();
            game.Deck = _sharedGameService.GenerateDeck(game.Theme);
            _sharedGameService.ShuffleDeck(game.Deck);
            _sharedGameService.EnsurePlayableBoard(game);

            var host = new Player(hostId, hostName);
            game.AddPlayer(host);

            return game;
        }

        public MultiplayerGameViewModel GetMultiplayerGameViewModel(MultiplayerGame game, string playerId)
        {
            var allSets = _sharedGameService.FindAllSets(game.VisibleCards);
            var player = game.GetPlayer(playerId);
            var winner = game.GetWinner();

            return new MultiplayerGameViewModel
            {
                RoomCode = game.RoomCode,
                VisibleCards = game.VisibleCards,
                ChatEntries = game.ChatLogEntries.OrderByDescending(e => e.Timestamp).ToList(),
                CurrentPlayerIndex = player != null ? game.Players.IndexOf(player) + 1 : 1,
                ElapsedTime = game.ElapsedTime,
                RemainingCards = game.RemainingCards,
                AvailableSets = allSets.Count,
                IsGameComplete = game.IsCompleted,
                GameEndMessage = game.IsCompleted ? "Game Over! All sets found!" : string.Empty,
                WinnerDisplayName = winner?.Name ?? string.Empty,
                Theme = game.Theme,
                GameId = game.GameId,
                Players = game.GetLeaderboard(),
                CurrentPlayerId = playerId,
                IsGameStarted = game.IsStarted,
                IsGameCompleted = game.IsCompleted,
                Winner = winner,
                Leaderboard = game.GetLeaderboard(),
                CurrentPlayerName = player?.Name ?? string.Empty,
                IsHost = game.IsPlayerHost(playerId)
            };
        }

        public WaitingRoomViewModel GetWaitingRoomViewModel(MultiplayerGame game, string playerId)
        {
            var player = game.GetPlayer(playerId);

            return new WaitingRoomViewModel
            {
                RoomCode = game.RoomCode,
                Players = game.Players,
                IsHost = game.IsPlayerHost(playerId),
                CurrentPlayerId = playerId,
                CurrentPlayerName = player?.Name ?? string.Empty
            };
        }

        public (bool IsValid, string Message, List<Card> ValidatedCards, int NewScore) ValidateMultiplayerSet(MultiplayerGame game, string playerId, List<int> cardIndices)
        {
            var player = game.GetPlayer(playerId);
            if (player == null)
            {
                return (false, "Player not found", new List<Card>(), 0);
            }

            if (cardIndices.Count != 3 || cardIndices.Any(i => i < 0 || i >= game.VisibleCards.Count))
            {
                return (false, "Invalid card selection", new List<Card>(), player.Score);
            }

            var selectedCards = cardIndices.Select(i => game.VisibleCards[i]).ToList();
            var isValid = _sharedGameService.IsValidSet(selectedCards);

            if (isValid)
            {
                player.Score++;

                var playerIndex = game.Players.IndexOf(player) + 1;
                var chatEntry = ChatEntry.CreateSetEntry(playerIndex, selectedCards, player.Name);
                game.ChatLogEntries.Add(chatEntry);

                var indicesToRemove = cardIndices.OrderByDescending(i => i).ToList();
                foreach (var index in indicesToRemove)
                {
                    game.VisibleCards.RemoveAt(index);
                }

                _sharedGameService.EnsurePlayableBoard(game);

                if (game.RemainingCards == 0 && _sharedGameService.FindAllSets(game.VisibleCards).Count == 0)
                {
                    game.IsCompleted = true;
                    game.WinnerId = game.GetWinner()?.Id;
                }

                return (true, $"{player.Name} found a valid set! +1 point", selectedCards, player.Score);
            }

            return (false, $"{player.Name} - Not a valid set", selectedCards, player.Score);
        }
    }
}
