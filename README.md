# Jaeger + OpenTelemetry + .NET 8 Demo

This project demonstrates how to integrate OpenTelemetry with Jaeger for distributed tracing in a .NET 8 Web API application.

## 🎯 Learning Objectives

- Understand OpenTelemetry fundamentals
- Set up Jaeger for distributed tracing
- Implement custom spans and attributes
- Monitor API performance and behavior

## 📋 Prerequisites

- .NET 8 SDK
- Docker Desktop
- Visual Studio Code (optional)

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

### 2. Run the .NET Application

Build and run the application:

```bash
dotnet build JaegerDemo.Api.csproj
dotnet run --project JaegerDemo.Api.csproj
```

The API will be available at:
- **HTTPS**: https://localhost:7299
- **HTTP**: http://localhost:5182

### 3. Generate Some Traces

Use the provided HTTP file (`JaegerDemo.Api.http`) or make requests manually:

```bash
# Get weather forecast
curl https://localhost:7299/weatherforecast -k

# Get weather for a specific city
curl https://localhost:7299/weather/London -k

# Bulk weather processing
curl -X POST https://localhost:7299/weather/bulk -k \
  -H "Content-Type: application/json" \
  -d '{"cities": ["New York", "London", "Tokyo"]}'
```

### 4. View Traces in Jaeger

1. Open your browser and navigate to http://localhost:16686
2. Select "JaegerDemo.Api" from the service dropdown
3. Click "Find Traces" to see your application traces

## 📊 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/weatherforecast` | Get a 5-day weather forecast |
| GET | `/weather/{city}` | Get current weather for a specific city |
| POST | `/weather/bulk` | Process weather data for multiple cities |

## 🔍 Understanding the Traces

### Automatic Instrumentation
The application automatically traces:
- **HTTP requests** (ASP.NET Core instrumentation)
- **HTTP client calls** (HttpClient instrumentation)

### Custom Spans
The application creates custom spans for:
- `generate-weather-forecast`: Weather forecast generation
- `get-weather-for-city`: Single city weather processing
- `external-weather-api-call`: Simulated external API calls
- `bulk-weather-processing`: Bulk processing operations
- `process-city-{cityName}`: Individual city processing

### Span Attributes
Each span includes relevant attributes:
- `forecast.count`: Number of forecast days
- `city`: City name being processed
- `api.endpoint`: External API endpoint name
- `cities.count`: Number of cities in bulk processing
- `api.response.temperature`: Temperature from external API

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
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("JaegerDemo.Api")
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "localhost";
                options.AgentPort = 14268;
            });
    });
```

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

### What are Traces?
Traces represent the journey of a request through your system, composed of multiple spans.

## 🔧 Troubleshooting

### Jaeger UI not accessible
- Ensure Docker container is running: `docker ps`
- Check if port 16686 is available
- Restart container: `docker-compose restart`

### No traces appearing in Jaeger
- Verify the application is making requests
- Check application logs for OpenTelemetry errors
- Ensure Jaeger agent port (14268) is accessible

### Build errors
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
- Check NuGet sources: `dotnet nuget list source`

## 🚀 Next Steps

1. **Add More Instrumentation**: Try adding database or Redis instrumentation
2. **Custom Metrics**: Implement OpenTelemetry metrics alongside tracing
3. **Structured Logging**: Add structured logging with OpenTelemetry
4. **Production Setup**: Configure OpenTelemetry for production environments
5. **Multiple Services**: Create multiple services to see distributed tracing

## 📚 Additional Resources

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [ASP.NET Core OpenTelemetry](https://learn.microsoft.com/en-us/aspnet/core/log-mon/opentelemetry)