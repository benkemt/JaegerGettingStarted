using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using JaegerDemo.Api;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed logging for OpenTelemetry
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Add DbContext with SQL Server
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherDb")));

// Create an ActivitySource BEFORE OpenTelemetry configuration
var activitySource = new ActivitySource("JaegerDemo.Api");

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "JaegerDemo.Api", serviceVersion: "1.0.0"))
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation(options =>
            {
                // Automatically record exceptions in ASP.NET Core request spans
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()  // EF Core queries will be traced automatically
            .AddSource("JaegerDemo.Api")
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Endpoint = new Uri("http://localhost:4317");
            });
    });

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.Migrate();  // Apply migrations
    app.Logger.LogInformation("Database migrated/created successfully");
}

// Log startup
app.Logger.LogInformation("Application starting with OpenTelemetry configured");

// Add exception tracing middleware - automatically records ALL unhandled exceptions
app.UseExceptionTracing();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("generate-weather-forecast");
    activity?.SetTag("forecast.count", 5);

    await Task.Delay(Random.Shared.Next(50, 200));
    
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    activity?.SetTag("forecast.generated", true);
    activity?.SetSuccess();  // Using extension method
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/weather/{city}", async (string city, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-weather-for-city");
    activity?.SetTag("city", city);
    
    using var apiActivity = activitySource.StartActivity("external-weather-api-call");
    apiActivity?.SetTag("api.endpoint", "external-weather-service");

    if (city == "Tokyo")
    {
        // Middleware will catch and record this automatically!
        throw new InvalidOperationException("Bad Town");
    }

    await Task.Delay(Random.Shared.Next(100, 500));
    
    var temperature = Random.Shared.Next(-10, 40);
    var summary = summaries[Random.Shared.Next(summaries.Length)];
    
    apiActivity?.SetTag("api.response.temperature", temperature);
    apiActivity?.SetTag("api.response.summary", summary);
    apiActivity?.SetSuccess();  // Using extension method
    
    activity?.SetSuccess();  // Using extension method
    
    return new WeatherForecast(
        DateOnly.FromDateTime(DateTime.Now),
        temperature,
        summary
    );
})
.WithName("GetWeatherForCity");

app.MapPost("/weather/record", async (WeatherRecordRequest request, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("save-weather-record");
    activity?.SetTag("city", request.City);

    logger.LogInformation("Saving weather record for {City}", request.City);

    var record = new WeatherRecord
    {
        City = request.City,
        Temperature = request.Temperature,
        Summary = request.Summary ?? summaries[Random.Shared.Next(summaries.Length)],
        RecordedAt = DateTime.UtcNow
    };

    db.WeatherRecords.Add(record);
    await db.SaveChangesAsync();  // Exception auto-recorded by middleware!

    activity?.SetTag("record.id", record.Id);
    activity?.SetSuccess();  // Using extension method

    return Results.Created($"/weather/record/{record.Id}", record);
})
.WithName("SaveWeatherRecord");

app.MapGet("/weather/records/{city}", async (string city, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-weather-records");
    activity?.SetTag("city", city);

    logger.LogInformation("Fetching weather records for {City}", city);

    var records = await db.WeatherRecords
        .Where(w => w.City == city)
        .OrderByDescending(w => w.RecordedAt)
        .Take(10)
        .ToListAsync();

    activity?.SetTag("records.count", records.Count);
    activity?.SetSuccess();  // Using extension method
    
    logger.LogInformation("Found {Count} records for {City}", records.Count, city);

    return records;
})
.WithName("GetWeatherRecords");

app.MapGet("/weather/latest/{city}", async (string city, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-latest-weather");
    activity?.SetTag("city", city);

    logger.LogInformation("Fetching latest weather for {City}", city);

    var latestRecord = await db.WeatherRecords
        .Where(w => w.City == city)
        .OrderByDescending(w => w.RecordedAt)
        .FirstOrDefaultAsync();

    if (latestRecord == null)
    {
        activity?.SetTag("record.found", false);
        activity?.SetSuccess();  // Using extension method
        return Results.NotFound(new { message = $"No weather records found for {city}" });
    }

    activity?.SetTag("record.found", true);
    activity?.SetTag("temperature", latestRecord.Temperature);
    activity?.SetSuccess();  // Using extension method

    return Results.Ok(latestRecord);
})
.WithName("GetLatestWeather");

app.MapGet("/weather/cities", async (WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-all-cities");

    logger.LogInformation("Fetching all cities");

    var cities = await db.WeatherRecords
        .Select(w => w.City)
        .Distinct()
        .OrderBy(c => c)
        .ToListAsync();

    activity?.SetTag("cities.count", cities.Count);
    activity?.SetSuccess();  // Using extension method

    return cities;
})
.WithName("GetAllCities");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherRecordRequest(string City, int Temperature, string? Summary);

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherRecord> WeatherRecords { get; set; } = null!;
}

public class WeatherRecord
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
