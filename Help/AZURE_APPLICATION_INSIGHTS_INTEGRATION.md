# Azure Application Insights with OpenTelemetry

This guide shows how to send OpenTelemetry traces from your .NET application to both Jaeger (local) and Azure Application Insights (cloud).

## What is Azure Application Insights?

Azure Application Insights is a cloud-based Application Performance Management (APM) service that provides:
- **Distributed Tracing**: Visualize end-to-end transactions across services
- **Performance Monitoring**: Track response times, failure rates, and dependencies
- **Exception Tracking**: Automatic exception capture and analysis
- **Live Metrics**: Real-time performance monitoring
- **Application Map**: Visual representation of service dependencies
- **Smart Detection**: AI-powered anomaly detection

## Setup Steps

### 1. Create Application Insights Resource in Azure

1. Go to the Azure Portal (https://portal.azure.com)
2. Click "Create a resource" ? Search for "Application Insights"
3. Fill in the details:
   - **Resource Group**: Create new or select existing
   - **Name**: Choose a unique name (e.g., `jaeger-demo-api`)
   - **Region**: Select your preferred region
   - **Resource Mode**: Workspace-based (recommended)
4. Click "Review + Create" ? "Create"

### 2. Get Your Connection String

After creation:
1. Navigate to your Application Insights resource
2. Go to **Overview** or **Properties**
3. Copy the **Connection String** (looks like: `InstrumentationKey=xxx;IngestionEndpoint=https://xxx.applicationinsights.azure.com/;...`)

### 3. Update appsettings.json

Replace the placeholder connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "YOUR_ACTUAL_CONNECTION_STRING_HERE"
  }
}
```

**Important**: Never commit real connection strings to source control! Use:
- `appsettings.Development.json` for local development
- Azure App Service Configuration for production
- User Secrets for local development: `dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "YOUR_CONNECTION_STRING"`

### 4. Dual Exporter Configuration

The current setup sends telemetry to **both** destinations:
- **Jaeger** (local): via OTLP exporter on `localhost:4317`
- **Application Insights** (cloud): via Azure Monitor exporter

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            // ... existing configuration ...
            .AddOtlpExporter(options => { ... })  // Sends to Jaeger
            // Azure Monitor automatically added by UseAzureMonitor()
    });
```

## Visualizing Traces in Azure

### Access Application Insights

1. Go to Azure Portal ? Your Application Insights resource
2. Explore the following sections:

#### **Application Map** (Recommended for Distributed Tracing)
- Navigate to: **Investigate** ? **Application Map**
- Shows visual service dependencies and call relationships
- Click on any component to see performance metrics and failures

#### **Transaction Search**
- Navigate to: **Investigate** ? **Transaction Search**
- Search and filter individual requests
- Click on any transaction to see the full end-to-end trace

#### **Performance**
- Navigate to: **Investigate** ? **Performance**
- View aggregated performance metrics
- Drill down into specific operations
- See slowest dependencies and database queries

#### **Failures**
- Navigate to: **Investigate** ? **Failures**
- View exception details with full stack traces
- See which operations are failing most frequently
- Includes the exceptions captured by your `ExceptionTracingMiddleware`

#### **Live Metrics**
- Navigate to: **Investigate** ? **Live Metrics**
- Real-time streaming telemetry
- See requests, failures, and performance as they happen

## Querying with KQL (Kusto Query Language)

Application Insights uses KQL for advanced queries:

### View All Traces
```kql
traces
| order by timestamp desc
| take 100
```

### View All Requests
```kql
requests
| where timestamp > ago(1h)
| order by timestamp desc
```

### View Failed Requests
```kql
requests
| where success == false
| order by timestamp desc
```

### View Exceptions
```kql
exceptions
| order by timestamp desc
| take 50
```

### View Database Dependencies (EF Core queries)
```kql
dependencies
| where type == "SQL"
| order by timestamp desc
| project timestamp, name, duration, resultCode, success
```

### View Custom Spans (Your ActivitySource)
```kql
dependencies
| where customDimensions.["span.kind"] == "internal"
| order by timestamp desc
```

### End-to-End Transaction Trace
```kql
union requests, dependencies, exceptions
| where operation_Id == "YOUR_OPERATION_ID"
| order by timestamp asc
```

## Key Features Available

### 1. **Automatic Instrumentation** (Same as Jaeger)
- ASP.NET Core HTTP requests
- HttpClient calls
- Entity Framework Core database queries
- Exceptions (via middleware)

### 2. **Custom Spans with Tags**
All your custom `ActivitySource` spans appear in Application Insights:
```csharp
using var activity = activitySource.StartActivity("get-weather-for-city");
activity?.SetTag("city", city);
activity?.SetSuccess();
```

### 3. **Exception Tracking**
Your `ExceptionTracingMiddleware` exceptions are automatically captured and displayed in the Failures blade.

### 4. **Performance Baselines**
Application Insights learns your application's normal behavior and can alert on anomalies.

### 5. **Alerting**
Set up alerts for:
- High error rates
- Slow response times
- Dependency failures
- Custom metrics

## Comparison: Jaeger vs Application Insights

| Feature | Jaeger | Application Insights |
|---------|--------|---------------------|
| **Hosting** | Self-hosted (Docker) | Cloud-based (Azure) |
| **Cost** | Free | Pay-as-you-go |
| **Setup** | Quick, local | Requires Azure account |
| **Retention** | Limited by storage | 90 days default (configurable) |
| **Alerting** | No | Yes, built-in |
| **Analytics** | Basic search | Advanced KQL queries |
| **Smart Detection** | No | Yes (AI-powered) |
| **Live Metrics** | No | Yes |
| **Integration** | Limited | Azure ecosystem |
| **Best For** | Development & Learning | Production monitoring |

## Best Practice: Conditional Exporter

For production, you might want to send only to Application Insights:

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(...)
            .AddAspNetCoreInstrumentation(...)
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("JaegerDemo.Api");

        // Only add Jaeger in Development
        if (builder.Environment.IsDevelopment())
        {
            tracerProviderBuilder.AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Endpoint = new Uri("http://localhost:4317");
            });
        }
    });
```

## Testing the Integration

1. **Start your application**: `dotnet run`
2. **Make some requests**: Use the `.http` file to trigger endpoints
3. **Check Jaeger**: http://localhost:16686 (immediate feedback)
4. **Check Application Insights**: Azure Portal (may take 1-3 minutes for data to appear)

### Test the Exception Tracking
```http
GET https://localhost:7041/weather/Tokyo
```

This will trigger an exception that appears in both:
- Jaeger spans (with error status)
- Application Insights Failures blade (with full exception details)

## Troubleshooting

### No Data in Application Insights?
1. **Check connection string**: Ensure it's correctly set
2. **Wait 2-3 minutes**: Initial ingestion can be slow
3. **Check logs**: Look for Azure Monitor errors in console output
4. **Verify SDK**: `dotnet list package` should show `Azure.Monitor.OpenTelemetry.AspNetCore`

### Data in Jaeger but not Application Insights?
- Verify network connectivity to Azure
- Check if firewall is blocking outbound HTTPS
- Verify connection string format

## Additional Resources

- [Azure Monitor OpenTelemetry Documentation](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore)
- [Application Insights Overview](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [KQL Quick Reference](https://learn.microsoft.com/azure/data-explorer/kql-quick-reference)

## Next Steps

- Set up **alerts** for critical failures
- Create **workbooks** for custom dashboards
- Enable **availability tests** for uptime monitoring
- Integrate with **Azure DevOps** or **GitHub Actions** for deployment tracking
