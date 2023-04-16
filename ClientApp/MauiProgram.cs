using ClientApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;
using Refit;

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
            builder.Services.AddRefitClient<IDataAccess>().ConfigureHttpClient(c =>
            {
                //c.BaseAddress = new Uri($"https://localhost:5001/mauiapi");

                // if using emulator
                c.BaseAddress = new Uri($"https://192.168.254.103:45458/mauiapi");
            });
            return builder.Build();
        }
    }
}