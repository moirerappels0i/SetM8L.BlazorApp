using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Services
{
    public interface ISharedGameService
    {
        List<Card> GenerateDeck(ThemeConfiguration theme);
        bool IsValidSet(List<Card> cards);
        List<List<Card>> FindAllSets(List<Card> visibleCards);
        List<Card> GetHint(List<Card> visibleCards);
        void ShuffleDeck(List<Card> deck);
        bool AreAllSetsFound(List<Card> visibleCards, List<Card> deck, int nextCardIndex);
        void UpdateCardColors(List<Card> cards, List<string> oldColors, List<string> newColors);
        void UpdateCardShapes(List<Card> cards, List<string> oldShapes, List<string> newShapes);
        void UpdateChatEntryColors(List<ChatEntry> chatEntries, List<string> oldColors, List<string> newColors);
        void UpdateChatEntryShapes(List<ChatEntry> chatEntries, List<string> oldShapes, List<string> newShapes);
        void EnsurePlayableBoard(Game game);
    }
}
