using SetCardGame.BlazorApp.Models.Game;

namespace SetCardGame.BlazorApp.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeConfiguration GetDefaultTheme()
        {
            return CreateThemeConfiguration();
        }

        public void ChangeTheme(ThemeConfiguration currentTheme, string themeType, int themeIndex)
        {
            if (themeType == "color")
            {
                currentTheme.ColorThemeIndex = themeIndex;
            }
            else if (themeType == "shape")
            {
                currentTheme.ShapeThemeIndex = themeIndex;
            }
        }

        public ThemeConfiguration CreateThemeConfiguration()
        {
            return new ThemeConfiguration
            {
                ColorThemes = new List<List<string>>
                {
                    new List<string> { "#ff0101", "#f1c40f", "#008002" },  // Red, Yellow, Green
                    new List<string> { "#ff0101", "#800080", "#008002" },  // Red, Purple, Green
                    new List<string> { "#1f73bc", "#f1c40f", "#008002" }   // Blue, Yellow, Green
                },
                ShapeThemes = new List<List<string>>
                {
                    new List<string> { "oval", "diamond", "squiggle" },
                    new List<string> { "square", "hearts", "squiggle" },
                    new List<string> { "oval", "triangle", "squiggle" }
                },
                ColorThemeIndex = 0,
                ShapeThemeIndex = 0
            };
        }
    }
}
