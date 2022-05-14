using BlazorWebAssemblyApp;
using BlazorWebAssemblyApp.Account;
using Core.Interface;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
}).AddAccountClaimsPrincipalFactory<UserAccountFactory>();

builder.Services.AddHttpClient("WebApi",
      client => client.BaseAddress = new Uri("https://localhost:7225"))
    .AddHttpMessageHandler(sp =>
    {
        var httpMessageHandler = sp.GetService<AuthorizationMessageHandler>()?
        .ConfigureHandler(authorizedUrls: new[] { "https://localhost:7225" });
        return httpMessageHandler ?? throw new NullReferenceException(nameof(AuthorizationMessageHandler));
    });

builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>();
    var weatherForecastServiceHttpClient = httpClient.CreateClient("WebApi");
    return new WeatherForecastService(weatherForecastServiceHttpClient);
});

await builder.Build().RunAsync();