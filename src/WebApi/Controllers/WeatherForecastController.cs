using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [EnableCors("local")]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IWeatherForecastRepository weatherForecastRepository;

        public WeatherForecastController(
            IWeatherForecastRepository weatherForecastRepository,
            ILogger<WeatherForecastController> logger)
        {
            this.weatherForecastRepository = weatherForecastRepository;
            this.logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return weatherForecastRepository.GetWeatherForecasts();
        }
    }
}