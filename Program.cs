using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

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
            .AddAspNetCoreInstrumentation()
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
    activity?.AddEvent(new ActivityEvent("Forecast generation started"));

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
    activity?.AddEvent(new ActivityEvent("Forecast generation completed"));
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/weather/{city}", async (string city, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-weather-for-city");
    activity?.SetTag("city", city);
    activity?.AddEvent(new ActivityEvent($"Processing weather for {city}"));
    
    try
    {
        using var apiActivity = activitySource.StartActivity("external-weather-api-call");
        apiActivity?.SetTag("api.endpoint", "external-weather-service");
        apiActivity?.AddEvent(new ActivityEvent("Calling external weather API"));

        if (city == "Tokyo")
        {
            var exception = new InvalidOperationException("Bad Town");
            throw exception;
        }

        await Task.Delay(Random.Shared.Next(100, 500));
        
        var temperature = Random.Shared.Next(-10, 40);
        var summary = summaries[Random.Shared.Next(summaries.Length)];
        
        apiActivity?.SetTag("api.response.temperature", temperature);
        apiActivity?.SetTag("api.response.summary", summary);
        apiActivity?.AddEvent(new ActivityEvent($"API returned temp: {temperature}°C"));
        
        activity?.SetStatus(ActivityStatusCode.Ok);
        
        return new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now),
            temperature,
            summary
        );
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        throw;
    }
})
.WithName("GetWeatherForCity");

// NEW: Save weather data to database
app.MapPost("/weather/record", async (WeatherRecordRequest request, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("save-weather-record");
    activity?.SetTag("city", request.City);
    activity?.AddEvent(new ActivityEvent($"Saving weather record for {request.City}"));

    logger.LogInformation("Saving weather record for {City}: {Temperature}°C", request.City, request.Temperature);

    try
    {
        var record = new WeatherRecord
        {
            City = request.City,
            Temperature = request.Temperature,
            Summary = request.Summary ?? summaries[Random.Shared.Next(summaries.Length)],
            RecordedAt = DateTime.UtcNow
        };

        db.WeatherRecords.Add(record);
        await db.SaveChangesAsync();

        activity?.SetTag("record.id", record.Id);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent($"Weather record saved with ID: {record.Id}"));
        logger.LogInformation("Weather record saved with ID: {Id}", record.Id);

        return Results.Created($"/weather/record/{record.Id}", record);
    }
    catch (Exception ex)
    {
        // Record the exception in the activity - THIS IS KEY for database errors!
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        activity?.AddEvent(new ActivityEvent($"Failed to save weather record: {ex.Message}"));
        
        logger.LogError(ex, "Failed to save weather record for {City}", request.City);
        
        // Re-throw to let ASP.NET Core handle the error response
        throw;
    }
})
.WithName("SaveWeatherRecord");

// NEW: Get weather records for a city from database
app.MapGet("/weather/records/{city}", async (string city, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-weather-records");
    activity?.SetTag("city", city);
    activity?.AddEvent(new ActivityEvent($"Fetching weather records for {city}"));

    logger.LogInformation("Fetching weather records for {City}", city);

    try
    {
        // This query will be traced by OpenTelemetry EF Core instrumentation
        var records = await db.WeatherRecords
            .Where(w => w.City == city)
            .OrderByDescending(w => w.RecordedAt)
            .Take(10)
            .ToListAsync();

        activity?.SetTag("records.count", records.Count);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent($"Found {records.Count} records for {city}"));
        logger.LogInformation("Found {Count} records for {City}", records.Count, city);

        return records;
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        activity?.AddEvent(new ActivityEvent($"Failed to fetch weather records: {ex.Message}"));
        
        logger.LogError(ex, "Failed to fetch weather records for {City}", city);
        throw;
    }
})
.WithName("GetWeatherRecords");

// NEW: Get latest weather for a city from database
app.MapGet("/weather/latest/{city}", async (string city, WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-latest-weather");
    activity?.SetTag("city", city);
    activity?.AddEvent(new ActivityEvent($"Fetching latest weather for {city}"));

    logger.LogInformation("Fetching latest weather for {City}", city);

    try
    {
        var latestRecord = await db.WeatherRecords
            .Where(w => w.City == city)
            .OrderByDescending(w => w.RecordedAt)
            .FirstOrDefaultAsync();

        if (latestRecord == null)
        {
            activity?.SetTag("record.found", false);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent($"No records found for {city}"));
            return Results.NotFound(new { message = $"No weather records found for {city}" });
        }

        activity?.SetTag("record.found", true);
        activity?.SetTag("temperature", latestRecord.Temperature);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent($"Latest record: {latestRecord.Temperature}°C"));

        return Results.Ok(latestRecord);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        activity?.AddEvent(new ActivityEvent($"Failed to fetch latest weather: {ex.Message}"));
        
        logger.LogError(ex, "Failed to fetch latest weather for {City}", city);
        throw;
    }
})
.WithName("GetLatestWeather");

// NEW: Get all cities with weather data
app.MapGet("/weather/cities", async (WeatherDbContext db, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("get-all-cities");
    activity?.AddEvent(new ActivityEvent("Fetching all cities with weather data"));

    logger.LogInformation("Fetching all cities");

    try
    {
        var cities = await db.WeatherRecords
            .Select(w => w.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        activity?.SetTag("cities.count", cities.Count);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent($"Found {cities.Count} cities"));

        return cities;
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
        activity?.AddEvent(new ActivityEvent($"Failed to fetch cities: {ex.Message}"));
        
        logger.LogError(ex, "Failed to fetch cities");
        throw;
    }
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
