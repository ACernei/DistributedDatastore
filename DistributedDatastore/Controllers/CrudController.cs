using System.Net;
using DistributedDatastore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedDatastore.Controllers;

[ApiController]
[Route("[controller]")]
public class CrudController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly IDataRepository dataRepository;
    private readonly IOptions<CrudOptions> options;
    private readonly ILogger<CrudController> logger;

    public CrudController(
        HttpClient httpClient,
        IDataRepository dataRepository,
        IOptions<CrudOptions> options,
        ILogger<CrudController> logger)
    {
        this.httpClient = httpClient;
        this.dataRepository = dataRepository;
        this.options = options;
        this.logger = logger;
    }

    // GET: api/Datas
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        this.logger.LogInformation($"{GetLocation()} {HttpContext.Request.Method} {HttpContext.Request.Path}");

        var data = new List<Data>();
        if (options.Value.IsLeader)
        {
            foreach (var server in options.Value.GetOtherServers())
            {
                try
                {
                    var response = await httpClient.GetAsync($"http://{server}/crud/");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<Data>>();
                        data.AddRange(responseData);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        return new JsonResult(this.dataRepository.ToList().Concat(data).DistinctBy(x => x.Id));
    }

    // GET: api/Datas/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetData(int id)
    {
        this.logger.LogInformation($"{GetLocation()} {HttpContext.Request.Method} {HttpContext.Request.Path}");

        if (options.Value.IsLeader)
        {
            foreach (var server in options.Value.GetOtherServers())
            {
                try
                {
                    var response = await httpClient.GetAsync($"http://{server}/crud/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<Data>();
                        return new JsonResult(responseData);
                    }
                }
                catch (Exception e)
                {
                }

            }
        }

        var data = this.dataRepository.Find(id);
        if (data == null)
            return NotFound();
        return new JsonResult(data);
    }

    // POST: api/Datas
    [HttpPost]
    public async Task<IActionResult> PostData(Data data)
    {
        this.logger.LogInformation($"{GetLocation()} {HttpContext.Request.Method} {HttpContext.Request.Path}");

        if (options.Value.IsLeader)
        {
            data.Id = this.dataRepository.GenerateId();
            var rnd = new Random();
            var chosenServers = options.Value.Servers
                .OrderBy(r => rnd.Next())
                .Take(options.Value.Servers.Count / 2 + 1).ToList();
            foreach (var server in chosenServers)
            {
                if (server == options.Value.Self)
                {
                    this.dataRepository.Add(data);
                    continue;
                }

                try
                {
                    await httpClient.PostAsJsonAsync($"http://{server}/crud/", data);
                }
                catch (Exception e)
                {
                }
            }
        }
        else
        {
            this.dataRepository.Add(data);
        }

        return new JsonResult(data);
    }

    // PUT: api/Datas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutData(int id, Data data)
    {
        data.Id = id;
        this.logger.LogInformation($"{GetLocation()} {HttpContext.Request.Method} {HttpContext.Request.Path}");

        if (options.Value.IsLeader)
        {
            Data responseData = default;
            foreach (var server in options.Value.GetOtherServers())
            {
                try
                {
                    var response = await httpClient.PutAsJsonAsync($"http://{server}/crud/{data.Id}", data);
                    if (response.IsSuccessStatusCode)
                    {
                        responseData = await response.Content.ReadFromJsonAsync<Data>();
                    }
                }
                catch (Exception e)
                {
                }
            }

            var existingData = this.dataRepository.Find(data.Id);
            if (existingData == null && responseData == null)
                return NotFound();
            if (existingData != null)
                this.dataRepository.Update(data);
        }
        else
        {
            var existingData = this.dataRepository.Find(data.Id);
            if (existingData == null)
                return NotFound();

            this.dataRepository.Update(data);
        }

        return new JsonResult(data);
    }

    // DELETE: api/Datas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteData(int id)
    {
        this.logger.LogInformation($"{GetLocation()} {HttpContext.Request.Method} {HttpContext.Request.Path}");

        if (options.Value.IsLeader)
        {
            var isSuccess = false;
            foreach (var server in options.Value.GetOtherServers())
            {
                try
                {
                    var response = await httpClient.DeleteAsync($"http://{server}/crud/{id}");
                    isSuccess = isSuccess | response.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                }
            }

            var existingData = this.dataRepository.Find(id);
            if (existingData == null && !isSuccess)
                return NotFound();
            if (existingData != null)
                this.dataRepository.Remove(existingData);
        }
        else
        {
            var existingData = this.dataRepository.Find(id);
            if (existingData == null)
                return NotFound();

            this.dataRepository.Remove(existingData);
        }

        return Ok();
    }

    private string GetLocation()
    {
        return this.options.Value.IsLeader ? "EXTERNAL" : "INTERNAL";
    }
}
