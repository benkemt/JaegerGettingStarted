# Entity Framework Core + OpenTelemetry Integration

## ? What Was Added

### 1. **NuGet Packages**
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite database provider
- `OpenTelemetry.Instrumentation.EntityFrameworkCore` - Automatic EF Core tracing

### 2. **Database Context**
- `WeatherDbContext` - DbContext for managing weather records
- `WeatherRecord` - Entity model for storing weather data

### 3. **New API Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/weather/record` | Save a new weather record to database |
| GET | `/weather/records/{city}` | Get last 10 weather records for a city |
| GET | `/weather/latest/{city}` | Get the most recent weather record for a city |
| GET | `/weather/cities` | Get all cities with weather data |

### 4. **OpenTelemetry Configuration**
Added EF Core instrumentation to automatically trace:
- Database queries (SELECT, INSERT, UPDATE, DELETE)
- Query execution time
- Connection operations
- SQL statements (visible in Jaeger spans)

## ?? What You'll See in Jaeger

When you use the database endpoints, Jaeger will show:

1. **Parent Span**: Your custom activity (e.g., "save-weather-record")
2. **Child Span**: EF Core database operation
   - Tags: `db.system` = `sqlite`
   - Tags: `db.name` = `main`
   - Tags: `db.statement` = actual SQL query
   - Duration: query execution time

## ?? How to Test

### 1. Start the Application
```bash
dotnet run
```

The database `weather.db` will be automatically created.

### 2. Save Some Weather Data
Use the `.http` file or cURL:

```bash
# Save weather for London
POST https://localhost:7064/weather/record
{
  "city": "London",
  "temperature": 18,
  "summary": "Mild"
}

# Save weather for Paris
POST https://localhost:7064/weather/record
{
  "city": "Paris",
  "temperature": 22,
  "summary": "Warm"
}
```

### 3. Query the Data
```bash
# Get all records for London
GET https://localhost:7064/weather/records/London

# Get latest weather for London
GET https://localhost:7064/weather/latest/London

# Get all cities
GET https://localhost:7064/weather/cities
```

### 4. View Traces in Jaeger
1. Open **http://localhost:16686**
2. Select **"JaegerDemo.Api"** service
3. Click **"Find Traces"**
4. Click on any trace to see:
   - Your custom activity spans
   - EF Core database query spans (child spans)
   - SQL statements in the span details
   - Query execution times

## ?? Example Trace Structure

```
GET /weather/latest/London
?? get-latest-weather (custom activity)
   ?? SELECT FROM WeatherRecords (EF Core)
      ?? db.system: sqlite
      ?? db.statement: SELECT ... FROM WeatherRecords WHERE ...
      ?? Duration: 5ms
```

## ?? Key Features

1. **Automatic SQL Tracing**: All EF Core queries are automatically traced
2. **Parent-Child Relationships**: Database queries appear as child spans of your custom activities
3. **Performance Monitoring**: See exactly how long each database query takes
4. **SQL Visibility**: View actual SQL statements in Jaeger (helpful for debugging)
5. **Distributed Tracing**: Track requests across your application and database

## ?? Tips

- The `AlwaysOnSampler` ensures all activities are traced (good for learning/development)
- In production, consider using `TraceIdRatioBased` sampler to sample a percentage of requests
- SQLite database file (`weather.db`) is created in the project root
- All timestamps are stored in UTC

## ?? Learning Points

1. **OpenTelemetry automatically instruments EF Core** - No manual span creation needed for queries
2. **Traces show the full request flow** - From HTTP request ? Your code ? Database
3. **Performance bottlenecks are visible** - See which database queries are slow
4. **SQL statements are captured** - Debug N+1 queries and optimization issues

Enjoy exploring distributed tracing with Entity Framework Core! ??
