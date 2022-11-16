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
    public async Task<ActionResult<IEnumerable<Data>>> GetData()
    {
        this.logger.LogInformation($"{HttpContext.Request.Method} FROM {HttpContext.Request.Path}");

        if (options.Value.IsLeader)
            foreach (var port in options.Value.ServerPorts)
                await httpClient.GetAsync($"http://localhost:{port}/crud/");

        return this.dataRepository.ToList();
    }

    // GET: api/Datas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Data>> GetData(int id)
    {
        this.logger.LogInformation($"{HttpContext.Request.Method} FROM {HttpContext.Request.Path}");

        var data = this.dataRepository.Find(id);
        if (data == null)
            return NotFound();

        if (options.Value.IsLeader)
            foreach (var port in options.Value.ServerPorts)
                await httpClient.GetAsync($"http://localhost:{port}/crud/{id}");

        return data;
    }

    // PUT: api/Datas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutData(int id, Data data)
    {
        this.logger.LogInformation($"{HttpContext.Request.Method} FROM {HttpContext.Request.Path}");

        if (id != data.Id)
            return BadRequest();

        var existingData = this.dataRepository.Find(id);
        if (existingData == null)
            return NotFound();

        this.dataRepository.Update(data);

        if (options.Value.IsLeader)
            foreach (var port in options.Value.ServerPorts)
                await httpClient.PutAsJsonAsync($"http://localhost:{port}/crud/{id}", data);

        return NoContent();
    }

    // POST: api/Datas
    [HttpPost]
    public async Task<ActionResult<Data>> PostData(Data data)
    {
        this.logger.LogInformation($"{HttpContext.Request.Method} FROM {HttpContext.Request.Path}");

        var existingData = this.dataRepository.Find(data.Id);
        if (existingData != null)
            return BadRequest();

        this.dataRepository.Add(data);

        if (options.Value.IsLeader)
            foreach (var port in options.Value.ServerPorts)
                await httpClient.PostAsJsonAsync($"http://localhost:{port}/crud/", data);

        return CreatedAtAction(nameof(GetData), new { id = data.Id }, data);
    }

    // DELETE: api/Datas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteData(int id)
    {
        this.logger.LogInformation($"{HttpContext.Request.Method} FROM {HttpContext.Request.Path}");

        var data = this.dataRepository.Find(id);
        if (data == null)
            return NotFound();

        this.dataRepository.Remove(data);

        if (options.Value.IsLeader)
            foreach (var port in options.Value.ServerPorts)
                await httpClient.DeleteAsync($"http://localhost:{port}/crud/{id}");

        return NoContent();
    }
}
