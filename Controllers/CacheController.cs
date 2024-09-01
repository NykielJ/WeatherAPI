using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : Controller
    {
        private readonly IDistributedCache _cache;

        public CacheController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Message = "Cache is running",
                DateTime = DateTime.Now

            });
        }

        [HttpGet("get/{key}")]
        public async Task<IActionResult> GetCacheValue(string key)
        {
            var value = await _cache.GetStringAsync(key);

            if (value == null)
            {
                return NotFound(new { Message = $"No cache entry found for key: {key}" });
            }

            return Ok(new { Key = key, Value = value });
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetCacheValue([FromBody] CacheItem item)
        {
            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
            {
                return BadRequest("Key and value are required.");
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(item.Key, item.Value, options);
            return Ok(new { Message = $"Cache item with key: {item.Key} has been set." });
        }

        [HttpDelete("remove/{key}")]
        public async Task<IActionResult> RemoveCacheValue(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value == null) {
                return NotFound(new { Message = $"No cache entry found for key: {key}" });
            }
            _cache.Remove(key);
            return Ok(new { Message = $"Cache item with key: {key} has been removed." });
        }
    }

    public class CacheItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
