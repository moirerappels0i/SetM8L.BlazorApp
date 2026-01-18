using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SetCardGame.BlazorApp;
using SetCardGame.BlazorApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register game services
builder.Services.AddScoped<ISharedGameService, SharedGameService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

await builder.Build().RunAsync();
