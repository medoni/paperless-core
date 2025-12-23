using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace PLC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "0.1.0";
        var buildDate = System.IO.File.GetLastWriteTimeUtc(assembly.Location);

        var response = new
        {
            version = "0.1.0-dev",
            apiVersion = "v1",
            buildDate = buildDate,
            dotnetVersion = Environment.Version.ToString(),
            platform = Environment.OSVersion.Platform.ToString(),
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return Ok(response);
    }
}
