using Core.Interfaces;
using Core.Models;
using System.Text.Json;

namespace Service.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient httpClient;
        private readonly bool useAccessToken;

        public WeatherForecastService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            var weatherForecasts = await httpClient.GetStreamAsync($"WeatherForecast")
                .ConfigureAwait(false);

            return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>>
                (weatherForecasts, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                .ConfigureAwait(false);
        }
    }
}
