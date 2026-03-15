namespace SetCardGame.BlazorApp.Services;

public interface IPushNotificationService
{
    /// <summary>
    /// Initialize the push notification system (registers service worker).
    /// </summary>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Check if the app is running in standalone mode (installed on home screen).
    /// </summary>
    Task<bool> IsStandaloneModeAsync();

    /// <summary>
    /// Check if push notifications are supported on this browser/device.
    /// </summary>
    Task<bool> IsPushSupportedAsync();

    /// <summary>
    /// Get the current notification permission state ('default', 'granted', 'denied', 'unsupported').
    /// </summary>
    Task<string> GetPermissionStateAsync();

    /// <summary>
    /// Check if there is an existing active push subscription.
    /// </summary>
    Task<bool> IsSubscribedAsync();

    /// <summary>
    /// Request permission and subscribe to push notifications.
    /// Must be called from a user gesture (click/tap).
    /// </summary>
    Task<PushSubscriptionResult> SubscribeAsync();

    /// <summary>
    /// Unsubscribe from push notifications.
    /// </summary>
    Task<bool> UnsubscribeAsync();

    /// <summary>
    /// Send a test notification to verify setup.
    /// </summary>
    Task<bool> SendTestNotificationAsync();
}

public class PushSubscriptionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Endpoint { get; set; }
    public string? P256dh { get; set; }
    public string? Auth { get; set; }
}
