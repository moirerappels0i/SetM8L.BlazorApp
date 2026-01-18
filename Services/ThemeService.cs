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
                    new List<string> { "#ff0101", "#f1c40f", "#008002" },
                    new List<string> { "#800080", "#008002", "#ff0101" },
                    new List<string> { "#1f73bc", "#0da215", "#dd9427" }
                },
                ShapeThemes = new List<List<string>>
                {
                    new List<string> { "oval", "diamond", "squiggle" },
                    new List<string> { "hearts", "squiggle", "triangle" },
                    new List<string> { "squiggle", "hearts", "triangle" }
                },
                ColorThemeIndex = 0,
                ShapeThemeIndex = 0
            };
        }
    }
}
