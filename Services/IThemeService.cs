using SetCardGame.BlazorApp.Models.Game;

namespace SetCardGame.BlazorApp.Services
{
    public interface IThemeService
    {
        ThemeConfiguration GetDefaultTheme();
        void ChangeTheme(ThemeConfiguration currentTheme, string themeType, int themeIndex);
        ThemeConfiguration CreateThemeConfiguration();
    }
}
