using Microsoft.AspNetCore.Mvc;
using NisFitnessPro.Models;
using NisFitnessPro.Services;

namespace NisFitnessPro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FitnessController : ControllerBase
{
    private readonly RedisService _redisService;

    public FitnessController(RedisService redisService)
    {
        _redisService = redisService;
    }

    [HttpPost("activity")]
    public async Task<IActionResult> PostActivity([FromBody] RunnerActivity activity)
    {
        var result = await _redisService.SaveActivityAsync(activity);
        return result ? Ok() : BadRequest("Greška pri upisu u Redis.");
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        return Ok(new
        {
            Leaderboard = await _redisService.GetTopRunnersAsync(),
            Feed = await _redisService.GetActivityFeedAsync(),
            Record = await _redisService.GetGlobalRecordAsync()
        });
    }

    [HttpGet("radar")]
    public async Task<IActionResult> GetRadar([FromQuery] double lat, [FromQuery] double lon)
    {
        var nearby = await _redisService.GetNearbyRunnersAsync(lat, lon);
        return Ok(nearby.Select(n => new { Username = n.Member.ToString(), Distance = n.Distance }));
    }
}