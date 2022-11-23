using System.Net;
using Broker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Broker.Controllers;

[ApiController]
[Route("[controller]")]
public class CrudController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly IOptions<CrudOptions> options;
    private readonly ILogger<CrudController> logger;
    // private Dictionary<string, bool> leaderStatus;

    public CrudController(
        HttpClient httpClient,
        IOptions<CrudOptions> options,
        ILogger<CrudController> logger)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.logger = logger;

        // this.leaderStatus = new Dictionary<string, bool> { { this.options.Value.Leader, true } };
        // this.options.Value.Servers.ForEach(x => this.leaderStatus.Add(x, false));
    }

    // GET: api/crud
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        try
        {
            var response = await httpClient.GetAsync($"http://{this.options.Value.Leader}/crud/");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<Data>>();
                return new JsonResult(responseData);
            }
        }
        catch (HttpRequestException)
        {
            ChangeLeader();

            var response = await httpClient.GetAsync($"http://{this.options.Value.Leader}/crud/");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<Data>>();
                return new JsonResult(responseData);
            }
        }


        return BadRequest();
    }

    // GET: api/crud/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetData(int id)
    {
        try
        {
            var response = await httpClient.GetAsync($"http://{this.options.Value.Leader}/crud/{id}");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }
        catch (HttpRequestException e)
        {
            ChangeLeader();

            var response = await httpClient.GetAsync($"http://{this.options.Value.Leader}/crud/{id}");
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }

        return BadRequest();
    }

    // POST: api/crud
    [HttpPost]
    public async Task<IActionResult> PostData(Data data)
    {
        this.logger.LogError($"{this.options.Value.Leader}");

        try
        {
            var response = await httpClient.PostAsJsonAsync($"http://{this.options.Value.Leader}/crud/", data);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }
        catch (HttpRequestException e)
        {
            await ChangeLeader();

            var response = await httpClient.PostAsJsonAsync($"http://{this.options.Value.Leader}/crud/", data);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }

        return BadRequest();
    }



    // PUT: api/crud/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutData(int id, Data data)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"http://{this.options.Value.Leader}/crud/{id}", data);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }
        catch (HttpRequestException e)
        {
            await ChangeLeader();

            var response = await httpClient.PutAsJsonAsync($"http://{this.options.Value.Leader}/crud/{id}", data);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<Data>();
                return new JsonResult(responseData);
            }
        }

        return BadRequest();
    }

    // DELETE: api/crud/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteData(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"http://{this.options.Value.Leader}/crud/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
        }
        catch (Exception e)
        {
            await ChangeLeader();

            var response = await httpClient.DeleteAsync($"http://{this.options.Value.Leader}/crud/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
        }

        return BadRequest();
    }

    private async Task ChangeLeader()
    {
        foreach (var server in this.options.Value.Servers)
        {
            if (server == this.options.Value.Leader)
                continue;
            try
            {
                var response = await httpClient.GetAsync($"http://{server}/status");
                if (response.IsSuccessStatusCode)
                {
                    this.options.Value.Leader = server;
                    await httpClient.PostAsync($"http://{this.options.Value.Leader}/leader", null);
                    break;
                }
            }
            catch (HttpRequestException)
            {
            }
        }
    }
}
