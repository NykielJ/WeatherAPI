using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeatherAPI.Services
{
    public class WeatherService
    {
        private readonly IDistributedCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public WeatherService(IConfiguration configuration, HttpClient httpClient, IDistributedCache cache, ILogger<WeatherService> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["WeatherAPI:ApiKey"];
            _baseUrl = configuration["WeatherAPI:BaseUrl"];
        }

        public async Task<string> GetWeatherAsync(string cityCode)
        {
            var cacheKey = $"Weather_{cityCode}";
            string cachedData = null;

            try
            {
                // Sprawdź dane w cache
                cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation($"Cache hit for key: {cacheKey} at {DateTime.UtcNow}");
                    return cachedData;
                }

                var requestUrl = $"{_baseUrl}{cityCode}?unitGroup=metric&key={_apiKey}&contentType=json";
                _logger.LogInformation($"Cache miss for key: {cacheKey}. Fetching data from API at {DateTime.UtcNow}");

                var response = await _httpClient.GetStringAsync(requestUrl);

            
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                await _cache.SetStringAsync(cacheKey, response, cacheOptions);
                _logger.LogInformation($"Data cached for key: {cacheKey} at {DateTime.UtcNow}");

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error occurred while fetching data from API.");
                throw new Exception("Error occurred while fetching data from API.", httpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                throw;
            }
        }
    }
}
