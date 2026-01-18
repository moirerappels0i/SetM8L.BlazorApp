using SetCardGame.BlazorApp.Models.Game;
using SetCardGame.BlazorApp.Models.Players;

namespace SetCardGame.BlazorApp.Services
{
    public class SharedGameService : ISharedGameService
    {
        private readonly Random _random = new Random();

        public List<Card> GenerateDeck(ThemeConfiguration theme)
        {
            var deck = new List<Card>();

            foreach (var shape in theme.CurrentShapes)
            {
                foreach (var color in theme.CurrentColors)
                {
                    foreach (var fill in theme.FillTypes)
                    {
                        foreach (var number in theme.Numbers)
                        {
                            deck.Add(new Card(shape, color, fill, number));
                        }
                    }
                }
            }

            return deck;
        }

        public bool IsValidSet(List<Card> cards)
        {
            if (cards.Count != 3)
                return false;

            return IsValidProperty(cards, c => c.Shape) &&
                   IsValidProperty(cards, c => c.Color) &&
                   IsValidProperty(cards, c => c.Fill) &&
                   IsValidProperty(cards, c => c.Number.ToString());
        }

        private bool IsValidProperty(List<Card> cards, Func<Card, string> propertySelector)
        {
            var values = cards.Select(propertySelector).ToList();
            return values.Distinct().Count() == 1 || values.Distinct().Count() == 3;
        }

        public List<List<Card>> FindAllSets(List<Card> visibleCards)
        {
            var sets = new List<List<Card>>();

            for (int i = 0; i < visibleCards.Count - 2; i++)
            {
                for (int j = i + 1; j < visibleCards.Count - 1; j++)
                {
                    for (int k = j + 1; k < visibleCards.Count; k++)
                    {
                        var possibleSet = new List<Card> { visibleCards[i], visibleCards[j], visibleCards[k] };
                        if (IsValidSet(possibleSet))
                        {
                            sets.Add(possibleSet);
                        }
                    }
                }
            }

            return sets;
        }

        public List<Card> GetHint(List<Card> visibleCards)
        {
            var allSets = FindAllSets(visibleCards);
            return allSets.FirstOrDefault() ?? new List<Card>();
        }

        public void ShuffleDeck(List<Card> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        public bool AreAllSetsFound(List<Card> visibleCards, List<Card> deck, int nextCardIndex)
        {
            var availableSets = FindAllSets(visibleCards);
            if (availableSets.Count > 0)
            {
                return false;
            }

            if (nextCardIndex < deck.Count)
            {
                return false;
            }

            return true;
        }

        public void UpdateCardColors(List<Card> cards, List<string> oldColors, List<string> newColors)
        {
            foreach (var card in cards)
            {
                var oldColorIndex = oldColors.IndexOf(card.Color);
                if (oldColorIndex >= 0 && oldColorIndex < newColors.Count)
                {
                    card.Color = newColors[oldColorIndex];
                }
            }
        }

        public void UpdateCardShapes(List<Card> cards, List<string> oldShapes, List<string> newShapes)
        {
            foreach (var card in cards)
            {
                var oldShapeIndex = oldShapes.IndexOf(card.Shape);
                if (oldShapeIndex >= 0 && oldShapeIndex < newShapes.Count)
                {
                    card.Shape = newShapes[oldShapeIndex];
                }
            }
        }

        public void UpdateChatEntryColors(List<ChatEntry> chatEntries, List<string> oldColors, List<string> newColors)
        {
            foreach (var entry in chatEntries)
            {
                if (entry.Cards != null && entry.Cards.Count > 0)
                {
                    UpdateCardColors(entry.Cards, oldColors, newColors);
                }
            }
        }

        public void UpdateChatEntryShapes(List<ChatEntry> chatEntries, List<string> oldShapes, List<string> newShapes)
        {
            foreach (var entry in chatEntries)
            {
                if (entry.Cards != null && entry.Cards.Count > 0)
                {
                    UpdateCardShapes(entry.Cards, oldShapes, newShapes);
                }
            }
        }

        public void EnsurePlayableBoard(Game game)
        {
            var targetCount = Math.Max(12, game.VisibleCards.Count);

            while (game.VisibleCards.Count < targetCount && game.RemainingCards > 0)
            {
                var newCard = game.DealNextCard();
                if (newCard != null)
                {
                    game.VisibleCards.Add(newCard);
                }
                else
                {
                    break;
                }
            }

            while (FindAllSets(game.VisibleCards).Count == 0 && game.RemainingCards > 0)
            {
                var cardsToAdd = Math.Min(3, game.RemainingCards);
                var newCards = game.DealCards(cardsToAdd);
                game.VisibleCards.AddRange(newCards);

                if (newCards.Count < cardsToAdd)
                {
                    break;
                }
            }
        }
    }
}
