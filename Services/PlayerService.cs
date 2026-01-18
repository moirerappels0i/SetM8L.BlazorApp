using Microsoft.JSInterop;

namespace SetCardGame.BlazorApp.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IJSRuntime _jsRuntime;
        private string? _cachedPlayerId;
        private string? _cachedPlayerName;

        public PlayerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetPlayerId()
        {
            if (!string.IsNullOrEmpty(_cachedPlayerId))
                return _cachedPlayerId;

            try
            {
                _cachedPlayerId = await _jsRuntime.InvokeAsync<string>("FirebaseInterop.getUserId");
                return _cachedPlayerId ?? Guid.NewGuid().ToString();
            }
            catch
            {
                _cachedPlayerId = Guid.NewGuid().ToString();
                return _cachedPlayerId;
            }
        }

        public async Task<string> GetPlayerName()
        {
            if (!string.IsNullOrEmpty(_cachedPlayerName))
                return _cachedPlayerName;

            try
            {
                _cachedPlayerName = await _jsRuntime.InvokeAsync<string>("FirebaseInterop.getPlayerName");
                if (string.IsNullOrEmpty(_cachedPlayerName))
                {
                    _cachedPlayerName = "Player";
                }
                return _cachedPlayerName;
            }
            catch
            {
                return "Player";
            }
        }

        public async Task SetPlayerName(string name)
        {
            _cachedPlayerName = name;
            try
            {
                await _jsRuntime.InvokeVoidAsync("FirebaseInterop.setPlayerName", name);
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
