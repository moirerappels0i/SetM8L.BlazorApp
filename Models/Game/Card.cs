using System.Text.Json.Serialization;

namespace SetCardGame.BlazorApp.Models.Game
{
    public class Card
    {
        [JsonPropertyName("shape")]
        public string Shape { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        [JsonPropertyName("fill")]
        public string Fill { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        public Card() { }

        public Card(string shape, string color, string fill, int number)
        {
            Shape = shape;
            Color = color;
            Fill = fill;
            Number = number;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Card other)
            {
                return Shape == other.Shape &&
                       Color == other.Color &&
                       Fill == other.Fill &&
                       Number == other.Number;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Shape, Color, Fill, Number);
        }
    }
}
