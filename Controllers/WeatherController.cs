using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;

        public WeatherController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("{cityCode}")]
        public async Task<IActionResult> GetWeather(string cityCode)
        {
            var weatherData = await _weatherService.GetWeatherAsync(cityCode);
            return Ok(weatherData);
        }

    }
}
