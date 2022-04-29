using Blazor.WebAssembly.App;
using Core.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Service.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("webapi", (sp, client) =>
{
    client.BaseAddress = new Uri("https://localhost:7225");
});

builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>();
    var weatherForecastServiceHttpClient = httpClient.CreateClient("webapi");
    return new WeatherForecastService(weatherForecastServiceHttpClient);
});

await builder.Build().RunAsync();
