using BlazorHybridApp.Auth0;
using Core.Interface;
using Core.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Services;

namespace BlazorHybridApp
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

            builder.Services.AddAuthorizationCore();

            builder.Services.AddSingleton<TokenProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                return new Auth0AuthenticationStateProvider(new Auth0AuthenticationStateProviderOptions
                {
                    Domain = "<YOUR_AUTH0_DOMAIN>",
                    ClientId = "<YOUR_CLIENT_ID>",
                    Scope = "openid profile",

                    // https://github.com/dotnet/maui/issues/8382
                    // RedirectUri = "http://localhost/callback"

                    RedirectUri = "myapp://callback"

                }, tokenProvider);
            });

            builder.Services.AddHttpClient("webapi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7225");
            });

            builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("webapi");
                return new WeatherForecastService(httpClient, tokenProvider);
            });

            return builder.Build();
        }
    }
}