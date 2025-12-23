using Microsoft.AspNetCore.Mvc;

namespace PLC.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var response = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "0.1.0-dev",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        _logger.LogInformation("Health check requested");
        return Ok(response);
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Check dependencies (database, storage, etc.)
        // For now, always ready
        var response = new
        {
            status = "ready",
            timestamp = DateTime.UtcNow,
            checks = new
            {
                database = "not_implemented",
                storage = "not_implemented"
            }
        };

        return Ok(response);
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        // Simple liveness check
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }
}
