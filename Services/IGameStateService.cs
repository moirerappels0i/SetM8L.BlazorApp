using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;
using SetCardGame.BlazorApp.Models.ViewModels;

namespace SetCardGame.BlazorApp.Services
{
    public interface IGameStateService
    {
        // Solo game
        Game CreateNewSoloGame(ThemeConfiguration? existingTheme = null);
        GameViewModel GetSoloGameViewModel(Game game, string playerName);
        (bool IsValid, string Message, List<Card> ValidatedCards) ValidateSet(Game game, List<int> cardIndices, string playerName);
        void ChangeTheme(Game game, string themeType, int themeIndex);

        // Multiplayer game
        MultiplayerGame CreateNewMultiplayerGame(string hostId, string hostName);
        MultiplayerGameViewModel GetMultiplayerGameViewModel(MultiplayerGame game, string playerId);
        WaitingRoomViewModel GetWaitingRoomViewModel(MultiplayerGame game, string playerId);
        (bool IsValid, string Message, List<Card> ValidatedCards, int NewScore) ValidateMultiplayerSet(MultiplayerGame game, string playerId, List<int> cardIndices);
    }
}
