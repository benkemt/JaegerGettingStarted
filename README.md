# Jaeger + OpenTelemetry + .NET 9 Demo

This project demonstrates how to integrate OpenTelemetry with Jaeger for distributed tracing in a .NET 9 Web API application, including Entity Framework Core database operations.

## 🎯 Learning Objectives

- Understand OpenTelemetry fundamentals
- Set up Jaeger for distributed tracing
- Implement custom spans and attributes
- Monitor API performance and behavior
- Track database operations with EF Core instrumentation
- Handle exceptions with automatic tracing

## 📋 Prerequisites

- .NET 9 SDK
- Docker Desktop
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express
- Visual Studio 2022 or Visual Studio Code (optional)

## 🚀 Quick Start

### 1. Start Jaeger Container

First, start the Jaeger container using Docker Compose:

```bash
docker-compose up -d
```

This will start Jaeger with the following ports:
- **16686**: Jaeger UI (http://localhost:16686)
- **14268**: Jaeger agent endpoint
- **4317**: OTLP gRPC receiver
- **4318**: OTLP HTTP receiver

### 2. Start SQL Server LocalDB

Ensure SQL Server LocalDB is running:

```powershell
sqllocaldb start MSSQLLocalDB
```

### 3. Run the .NET Application

Build and run the application:

```bash
dotnet build JaegerDemo.Api.csproj
dotnet run --project JaegerDemo.Api.csproj
```

The database will be automatically created on first run using EF Core migrations.

The API will be available at:
- **HTTPS**: https://localhost:7064
- **HTTP**: http://localhost:5182

### 4. Generate Some Traces

Use the provided HTTP file (`JaegerDemo.Api.http`) or make requests manually:

```bash
# Get weather forecast
curl https://localhost:7064/weatherforecast -k

# Get weather for a specific city
curl https://localhost:7064/weather/London -k

# Save weather data to database
curl -X POST https://localhost:7064/weather/record -k \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "temperature": 18, "summary": "Mild"}'

# Get weather records for a city
curl https://localhost:7064/weather/records/London -k

# Get all cities with weather data
curl https://localhost:7064/weather/cities -k
```

### 5. View Traces in Jaeger

1. Open your browser and navigate to http://localhost:16686
2. Select "JaegerDemo.Api" from the service dropdown
3. Click "Find Traces" to see your application traces
4. Notice the database query spans within your traces!

## 📊 API Endpoints

### Weather Forecasts
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/weatherforecast` | Get a 5-day weather forecast |
| GET | `/weather/{city}` | Get current weather for a specific city |

### Database Operations (New!)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/weather/record` | Save a weather record to SQL Server database |
| GET | `/weather/records/{city}` | Get last 10 weather records for a specific city |
| GET | `/weather/latest/{city}` | Get the most recent weather record for a city |
| GET | `/weather/cities` | Get all cities with weather data |

### Example Requests

#### Save Weather Record
```json
POST /weather/record
{
  "city": "London",
  "temperature": 18,
  "summary": "Mild"
}
```

#### Get Weather Records
```bash
GET /weather/records/London
```

Response:
```json
[
  {
    "id": 1,
    "city": "London",
    "temperature": 18,
    "summary": "Mild",
    "recordedAt": "2025-01-24T16:30:45.123Z"
  }
]
```

## 🔍 Understanding the Traces

### Automatic Instrumentation
The application automatically traces:
- **HTTP requests** (ASP.NET Core instrumentation with automatic exception recording)
- **HTTP client calls** (HttpClient instrumentation)
- **Database queries** (Entity Framework Core instrumentation) - **NEW!**
  - SQL queries are visible in Jaeger spans
  - Query execution times are tracked
  - Database connection information is captured

### Activity Patterns: Activity.Current vs Child Spans

This demo demonstrates two different approaches to working with OpenTelemetry activities:

#### 1. Adding Tags to the Automatic Activity (`/weatherforecast`)

```csharp
app.MapGet("/weatherforecast", async (ILogger<Program> logger) =>
{
    // ASP.NET Core middleware already created an activity for this HTTP request
    // We're just adding custom tags to that existing activity
    Activity.Current?.SetTag("forecast.count", 5);
    
    await Task.Delay(Random.Shared.Next(50, 200));
    
    var forecast = Enumerable.Range(1, 5).Select(index => /* ... */).ToArray();
    
    Activity.Current?.SetSuccess();
    return forecast;
});
```

**What happens in Jaeger:**
- **ONE span**: `GET /weatherforecast` (created by ASP.NET Core middleware)
- The span includes our custom tags: `forecast.count=5`
- Clean and simple for straightforward endpoints

#### 2. Creating Child Spans (`/weather/{city}`)

```csharp
app.MapGet("/weather/{city}", async (string city, ILogger<Program> logger) =>
{
    // We're creating NEW child activities under the ASP.NET Core activity
    using var activity = activitySource.StartActivity("get-weather-for-city");
    activity?.SetTag("city", city);
    
    using var apiActivity = activitySource.StartActivity("external-weather-api-call");
    apiActivity?.SetTag("api.endpoint", "external-weather-service");
    
    await Task.Delay(Random.Shared.Next(100, 500));
    
    // ... rest of code
});
```

**What happens in Jaeger:**
- **Three spans** in a parent-child hierarchy:
  1. **Parent**: `GET /weather/{city}` (ASP.NET Core middleware)
  2. **Child 1**: `get-weather-for-city` (our custom span)
  3. **Child 2**: `external-weather-api-call` (nested under child 1)
- Better visibility into internal operations
- Useful for breaking down complex operations

#### When to Use Each Approach

| Approach | Use When | Benefits |
|----------|----------|----------|
| **Activity.Current** | Simple endpoints with no sub-operations | Cleaner traces, less overhead |
| **Child Spans** | Complex operations that need breakdown | Better observability, timing of sub-operations |

**Key Insight:** ASP.NET Core middleware **always** creates a root activity for every HTTP request. You're choosing whether to:
- Enhance it with tags (`Activity.Current`)
- Create child spans to break down the work (`activitySource.StartActivity()`)

### Custom Spans
The application creates custom spans for:
- `generate-weather-forecast`: Weather forecast generation
- `get-weather-for-city`: Single city weather processing
- `external-weather-api-call`: Simulated external API calls
- `save-weather-record`: Saving weather data to database - **NEW!**
- `get-weather-records`: Fetching weather records from database - **NEW!**
- `get-latest-weather`: Getting latest weather record - **NEW!**
- `get-all-cities`: Fetching all cities with data - **NEW!**

### Span Attributes
Each span includes relevant attributes:
- `forecast.count`: Number of forecast days
- `city`: City name being processed
- `api.endpoint`: External API endpoint name
- `record.id`: Database record ID - **NEW!**
- `records.count`: Number of records fetched - **NEW!**
- `record.found`: Whether a record was found - **NEW!**
- `temperature`: Temperature value - **NEW!**

### Database Spans (Automatic)
EF Core instrumentation automatically creates spans for:
- `INSERT` operations with SQL statements
- `SELECT` operations with SQL statements
- Database connection operations
- Tags include:
  - `db.system`: `mssql`
  - `db.name`: `WeatherDb`
  - `db.statement`: Actual SQL query executed

### Exception Tracking
All exceptions are automatically recorded in traces with:
- Exception type (e.g., `SqlException`, `InvalidOperationException`)
- Full error message
- Complete stack trace
- No manual try-catch blocks required!

## 🗄️ Database

### Technology
- **SQL Server LocalDB** for development
- **Entity Framework Core 9.0** for data access
- **Automatic migrations** on application startup

### Connection String
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "WeatherDb": "Server=(localdb)\\mssqllocaldb;Database=WeatherDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Database Schema
**WeatherRecords** table:
- `Id` (int, primary key)
- `City` (nvarchar)
- `Temperature` (int)
- `Summary` (nvarchar)
- `RecordedAt` (datetime2)

### View Database
Connect to `(localdb)\mssqllocaldb` using:
- SQL Server Object Explorer in Visual Studio
- Azure Data Studio
- SQL Server Management Studio (SSMS)

## 🛠️ Configuration

### OpenTelemetry Configuration

The OpenTelemetry configuration is in `Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("JaegerDemo.Api", "1.0.0"))
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;  // Automatic exception recording
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()  // Database tracing
            .AddSource("JaegerDemo.Api")
            .AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Endpoint = new Uri("http://localhost:4317");
            });
    });
```

### Key Features
- **AlwaysOnSampler**: All requests are sampled (good for development/learning)
- **Automatic exception recording**: No try-catch boilerplate needed
- **EF Core instrumentation**: Database queries automatically traced
- **Exception tracing middleware**: Captures all unhandled exceptions

### Jaeger Configuration

The Jaeger container is configured in `docker-compose.yml` with OTLP support enabled.

## 📝 Key Concepts for Beginners

### What is OpenTelemetry?
OpenTelemetry is an open-source observability framework that provides APIs, libraries, and tools to collect, process, and export telemetry data (traces, metrics, logs).

### What is Jaeger?
Jaeger is a distributed tracing system used for monitoring and troubleshooting microservices-based distributed systems.

### What are Spans?
Spans represent individual operations within a trace. Each span has:
- A name
- Start and end time
- Attributes (key-value pairs)
- Parent-child relationships
- Status (OK, ERROR)
- Events and exceptions

### What are Traces?
Traces represent the journey of a request through your system, composed of multiple spans.

### Database Tracing
With EF Core instrumentation, you can see:
- Which database queries are executed
- How long each query takes
- The actual SQL statements
- Database connection details
- Query parameters (with proper configuration)

## 🔧 Troubleshooting

### Jaeger UI not accessible
- Ensure Docker container is running: `docker ps`
- Check if port 16686 is available
- Restart container: `docker-compose restart`

### No traces appearing in Jaeger
- Verify the application is making requests
- Check application logs for OpenTelemetry errors
- Ensure Jaeger is receiving on port 4317 (gRPC)

### Database connection errors
- Ensure SQL Server LocalDB is running: `sqllocaldb info`
- Start LocalDB: `sqllocaldb start MSSQLLocalDB`
- Check connection string in `appsettings.json`

### Migration errors
- Drop database: `dotnet ef database drop`
- Restart application to recreate database

### Build errors
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
- Check NuGet sources: `dotnet nuget list source`

## 📚 Architecture & Best Practices

### Code Structure
- **ExceptionTracingMiddleware.cs**: Automatic exception recording for all endpoints
- **ActivityExtensions.cs**: Helper methods to reduce boilerplate (`SetSuccess()`, `RecordError()`)
- **ActivityExceptionFilter.cs**: Alternative filter-based exception handling (optional)

### Clean Code Pattern
The application uses minimal boilerplate:
- ❌ No try-catch blocks in endpoints (middleware handles it)
- ✅ Extension methods for common operations
- ✅ Automatic exception recording
- ✅ Focus on business logic

Example:
```csharp
app.MapPost("/weather/record", async (request, db) =>
{
    using var activity = activitySource.StartActivity("save-weather-record");
    activity?.SetTag("city", request.City);
    
    var record = new WeatherRecord { ... };
    await db.SaveChangesAsync();  // Exceptions auto-traced!
    
    activity?.SetSuccess();  // Extension method
    return Results.Created($"/weather/record/{record.Id}", record);
});
```

## 🚀 Next Steps

1. **Explore Traces**: Open Jaeger and explore the database query spans
2. **Test Error Scenarios**: Stop SQL Server and see exception details in Jaeger
3. **Add Custom Metrics**: Implement OpenTelemetry metrics alongside tracing
4. **Add Logging**: Integrate structured logging with OpenTelemetry
5. **Production Setup**: Configure sampling and exporters for production
6. **Multiple Services**: Create multiple services to see distributed tracing across services
7. **Azure Deployment**: Deploy to Azure with Azure SQL Database

## 📚 Additional Resources

### Documentation Created
- **EF_CORE_OPENTELEMETRY.md**: Entity Framework Core integration guide
- **SQL_SERVER_MIGRATION.md**: Complete SQL Server setup guide
- **QUICK_START_SQL_SERVER.md**: Quick verification steps
- **ERROR_HANDLING_GUIDE.md**: Exception tracing guide with examples
- **REDUCING_BOILERPLATE.md**: Best practices for clean code
- **CODE_SIMPLIFICATION_APPLIED.md**: Before/after code comparisons

### External Resources
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [ASP.NET Core OpenTelemetry](https://learn.microsoft.com/en-us/aspnet/core/log-mon/opentelemetry)
- [EF Core OpenTelemetry](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/opentelemetry)

## 🎓 What You'll Learn

By exploring this demo, you'll understand:
- ✅ How to set up OpenTelemetry in .NET 9
- ✅ How to integrate Jaeger for distributed tracing
- ✅ How to create custom spans and add attributes
- ✅ How to automatically trace database operations
- ✅ How to handle exceptions with automatic recording
- ✅ How to reduce observability boilerplate code
- ✅ How to debug performance issues using traces
- ✅ How to track SQL queries and their execution time

## 🎉 Features Demonstrated

- ✅ **Automatic HTTP tracing** (ASP.NET Core)
- ✅ **Custom activity spans** with tags and events
- ✅ **Database query tracing** (Entity Framework Core)
- ✅ **Automatic exception recording** (no try-catch needed!)
- ✅ **SQL Server integration** with LocalDB
- ✅ **OTLP over gRPC** export to Jaeger
- ✅ **Clean code patterns** with extension methods
- ✅ **Error visualization** in Jaeger UI
- ✅ **Performance monitoring** of database queries

---

**Happy Tracing!** 🚀 If you have questions, check the documentation in the `Help/` folder or create an issue on GitHub.