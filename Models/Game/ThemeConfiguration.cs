using System.Text.Json.Serialization;

namespace SetCardGame.BlazorApp.Models.Game
{
    public class ThemeConfiguration
    {
        [JsonPropertyName("colorThemes")]
        public List<List<string>> ColorThemes { get; set; } = new List<List<string>>
        {
            new List<string> { "#ff0101", "#f1c40f", "#008002" },  // Red, Yellow, Green
            new List<string> { "#ff0101", "#800080", "#008002" },  // Red, Purple, Green
            new List<string> { "#1f73bc", "#f1c40f", "#008002" }   // Blue, Yellow, Green
        };

        [JsonPropertyName("shapeThemes")]
        public List<List<string>> ShapeThemes { get; set; } = new List<List<string>>
        {
            new List<string> { "oval", "diamond", "squiggle" },
            new List<string> { "square", "hearts", "squiggle" },
            new List<string> { "oval", "triangle", "squiggle" }
        };

        [JsonPropertyName("fillTypes")]
        public List<string> FillTypes { get; set; } = new List<string> { "solid", "striped", "outline" };

        [JsonPropertyName("numbers")]
        public List<int> Numbers { get; set; } = new List<int> { 1, 2, 3 };

        [JsonPropertyName("colorThemeIndex")]
        public int ColorThemeIndex { get; set; } = 0;

        [JsonPropertyName("shapeThemeIndex")]
        public int ShapeThemeIndex { get; set; } = 0;

        [JsonIgnore]
        public List<string> CurrentColors => ColorThemes[ColorThemeIndex];

        [JsonIgnore]
        public List<string> CurrentShapes => ShapeThemes[ShapeThemeIndex];
    }
}
