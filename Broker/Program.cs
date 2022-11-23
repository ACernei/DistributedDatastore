// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
//
// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

using Broker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddHttpClient("ConfiguredHttpMessageHandler").ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        // allow self signed certificates
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

builder.Services.AddLogging(c => c.AddSimpleConsole(options =>
{
    // options.TimestampFormat = "[HH:mm:ss.ffff] ";
    options.SingleLine = true;
    // options.ColorBehavior = LoggerColorBehavior.Disabled;
}));

// Read appsettings configuration
builder.Services.Configure<CrudOptions>(
    builder.Configuration.GetSection("Options"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
