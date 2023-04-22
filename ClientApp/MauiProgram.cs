using Blazored.LocalStorage;
using ClientApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Animations;
using MudBlazor.Services;
using Refit;
using System.Net;

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
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddScoped<HttpClient>();
            builder.Services.AddScoped<AppAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthHeaderHandler>();
            builder.Services.AddScoped<AuthenticationStateProvider>(options => options.GetRequiredService<AppAuthenticationStateProvider>());
            builder.Services.AddAuthorizationCore();
            builder.Services.AddMudServices();
            builder.Services.AddRefitClient<IDataAccess>().ConfigureHttpClient(c =>
            {
                if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    c.BaseAddress = new Uri($"http://10.0.2.2:5001/mauiapi");
                }
                else
                {
                    c.BaseAddress = new Uri($"http://localhost:5001/mauiapi");
                }
            }).AddHttpMessageHandler<AuthHeaderHandler>();
            return builder.Build();
        }
    }
}