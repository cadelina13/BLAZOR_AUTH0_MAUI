using ClientApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;

namespace ClientApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton(new Auth0Client(new()
            {
                Domain = "dev-auth0-maui-blazor.us.auth0.com",
                ClientId = "jhqaFhLQz7TeHyZp5y6XGxZsI7Qb4Yty",
                Scope = "openid profile",
                RedirectUri = "myapp://callback"
            }));
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, Auth0AuthenticationStateProvider>();
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}