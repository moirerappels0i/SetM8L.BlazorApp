using System.Text.Json;
using Microsoft.JSInterop;

namespace SetCardGame.BlazorApp.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized;

    private const string VapidPublicKey = "BJVrvhpMjsP79fU4cvF7DR3_aLQCi76idIRYo7RflbzgkgDTzs1jKwtdG4TuImva_CD4Mb1_xM2wBUgA28R0_aQ";

    public PushNotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _isInitialized = await _jsRuntime.InvokeAsync<bool>("PushNotifications.initialize");
            return _isInitialized;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Push notification init failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsStandaloneModeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotifications.isStandaloneMode");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsPushSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotifications.isPushSupported");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPermissionStateAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("PushNotifications.getPermissionState");
        }
        catch
        {
            return "unsupported";
        }
    }

    public async Task<bool> IsSubscribedAsync()
    {
        try
        {
            var existing = await _jsRuntime.InvokeAsync<string?>("PushNotifications.getExistingSubscription");
            return existing != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PushSubscriptionResult> SubscribeAsync()
    {
        try
        {
            var resultJson = await _jsRuntime.InvokeAsync<string>(
                "PushNotifications.requestPermissionAndSubscribe",
                VapidPublicKey);

            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            var success = root.GetProperty("success").GetBoolean();

            if (!success)
            {
                return new PushSubscriptionResult
                {
                    Success = false,
                    Error = root.TryGetProperty("error", out var err) ? err.GetString() : "Unknown error"
                };
            }

            var subscription = root.GetProperty("subscription");
            var endpoint = subscription.GetProperty("endpoint").GetString();
            var keys = subscription.GetProperty("keys");
            var p256dh = keys.GetProperty("p256dh").GetString();
            var auth = keys.GetProperty("auth").GetString();

            // Save subscription to Firebase for server-side push
            var userId = await _jsRuntime.InvokeAsync<string?>("FirebaseInterop.getUserId");
            if (userId != null)
            {
                await _jsRuntime.InvokeAsync<bool>(
                    "PushNotifications.saveSubscriptionToFirebase",
                    userId,
                    JsonSerializer.Serialize(subscription));
            }

            return new PushSubscriptionResult
            {
                Success = true,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            };
        }
        catch (Exception ex)
        {
            return new PushSubscriptionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<bool> UnsubscribeAsync()
    {
        try
        {
            var resultJson = await _jsRuntime.InvokeAsync<string>("PushNotifications.unsubscribe");
            using var doc = JsonDocument.Parse(resultJson);
            var success = doc.RootElement.GetProperty("success").GetBoolean();

            if (success)
            {
                // Remove from Firebase
                var userId = await _jsRuntime.InvokeAsync<string?>("FirebaseInterop.getUserId");
                if (userId != null)
                {
                    await _jsRuntime.InvokeAsync<bool>(
                        "PushNotifications.removeSubscriptionFromFirebase",
                        userId);
                }
            }

            return success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unsubscribe failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendTestNotificationAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotifications.showTestNotification");
        }
        catch
        {
            return false;
        }
    }
}
