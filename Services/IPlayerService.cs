namespace SetCardGame.BlazorApp.Services
{
    public interface IPlayerService
    {
        Task<string> GetPlayerId();
        Task<string> GetPlayerName();
        Task SetPlayerName(string name);
    }
}
