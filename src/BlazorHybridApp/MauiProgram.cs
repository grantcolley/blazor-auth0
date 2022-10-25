using BlazorHybridApp.Auth0;
using BlazorHybridApp.HttpDev;
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
            builder.Services.AddScoped<Auth0AuthenticationStateProviderOptions>();
            builder.Services.AddScoped<Auth0AuthenticationStateProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            {
                var tokenProvider = sp.GetRequiredService<TokenProvider>();
                var auth0AuthenticationStateProviderOptions = sp.GetRequiredService<Auth0AuthenticationStateProviderOptions>();

                auth0AuthenticationStateProviderOptions.Domain = "<YOUR_AUTH0_DOMAIN>";
                auth0AuthenticationStateProviderOptions.ClientId = "<YOUR_CLIENT_ID>";
                auth0AuthenticationStateProviderOptions.AdditionalProviderParameters.Add("audience", "<YOUR_AUDIENCE>");
                auth0AuthenticationStateProviderOptions.Scope = "openid profile";
                auth0AuthenticationStateProviderOptions.RoleClaim = "role";
                auth0AuthenticationStateProviderOptions.RedirectUri = "myapp://callback";
                //auth0AuthenticationStateProviderOptions.RedirectUri = "http://localhost/callback"; // https://github.com/dotnet/maui/issues/8382

                return sp.GetRequiredService<Auth0AuthenticationStateProvider>();
            });

#if DEBUG
            builder.Services.AddDevHttpClient("webapi", 7225);
#else
            builder.Services.AddHttpClient("webapi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7225");
            });
#endif

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