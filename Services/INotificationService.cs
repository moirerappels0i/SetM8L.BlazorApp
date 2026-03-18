namespace SetCardGame.BlazorApp.Services;

public interface INotificationService
{
    Task<bool> IsSupported();
    Task<bool> IsStandalone();
    Task<string> GetPermission();
    Task<string> RequestPermission();
    Task<bool> IsEnabled();
    Task SetEnabled(bool enabled);
    Task<bool> ShowNotification(string title, string body, string? url = null);
    Task<bool> ShowTestNotification(string title, string body);
    Task<bool> IsInAppEnabled();
    Task SetInAppEnabled(bool enabled);
}
