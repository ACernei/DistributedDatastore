using DistributedDatastore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedDatastore.Controllers;

[ApiController]
[Route("")]
public class StateController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly IOptions<CrudOptions> options;
    private readonly ILogger<StateController> logger;

    public StateController(
        HttpClient httpClient,
        IOptions<CrudOptions> options,
        ILogger<StateController> logger)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.logger = logger;
    }

    [HttpGet("/status")]
    public async Task<IActionResult> HealthCheck()
    {
        this.logger.LogInformation($"INTERNAL I AM ALIVE");

        return Ok();
    }

    [HttpPost("/leader")]
    public async Task<IActionResult> BecomeLeader()
    {
        this.logger.LogInformation($"INTERNAL I AM THE NEW LEADER");

        this.options.Value.IsLeader = true;
        return Ok();
    }
}
