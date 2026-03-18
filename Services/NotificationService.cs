using Microsoft.JSInterop;

namespace SetCardGame.BlazorApp.Services;

public class NotificationService : INotificationService
{
    private readonly IJSRuntime _jsRuntime;

    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsSupported()
    {
        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.isSupported");
    }

    public async Task<bool> IsStandalone()
    {
        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.isStandalone");
    }

    public async Task<string> GetPermission()
    {
        return await _jsRuntime.InvokeAsync<string>("GameInterop.notifications.getPermission");
    }

    public async Task<string> RequestPermission()
    {
        return await _jsRuntime.InvokeAsync<string>("GameInterop.notifications.requestPermission");
    }

    public async Task<bool> IsEnabled()
    {
        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.isEnabled");
    }

    public async Task SetEnabled(bool enabled)
    {
        await _jsRuntime.InvokeVoidAsync("GameInterop.notifications.setEnabled", enabled);
    }

    public async Task<bool> ShowNotification(string title, string body, string? url = null)
    {
        var enabled = await IsEnabled();
        if (!enabled) return false;

        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.show", title, body, null, url);
    }

    public async Task<bool> ShowTestNotification(string title, string body)
    {
        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.showTest", title, body);
    }

    public async Task<bool> IsInAppEnabled()
    {
        return await _jsRuntime.InvokeAsync<bool>("GameInterop.notifications.isInAppEnabled");
    }

    public async Task SetInAppEnabled(bool enabled)
    {
        await _jsRuntime.InvokeVoidAsync("GameInterop.notifications.setInAppEnabled", enabled);
    }
}
