using Microsoft.AspNetCore.Mvc;
using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace YourProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="Admin")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("{days}")]
        public IEnumerable<WeatherForecast> GetForecastForDays(int days)
        {
            if (days <= 0 || days > 30)
                throw new ArgumentException("Days must be between 1 and 30");

            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("analyze")]
        public IActionResult AnalyzeForecasts([FromBody] List<WeatherForecast> forecasts)
        {
            if (forecasts == null || !forecasts.Any())
                return BadRequest("No forecasts provided");

            var analysis = new
            {
                AverageTemperature = forecasts.Average(f => f.TemperatureC),
                MaxTemperature = forecasts.Max(f => f.TemperatureC),
                MinTemperature = forecasts.Min(f => f.TemperatureC),
                MostCommonSummary = forecasts
                    .GroupBy(f => f.Summary)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key
            };

            return Ok(analysis);
        }
    }
}