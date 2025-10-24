# Quick Start - SQL Server Setup

## ? **Verify Everything is Working**

### Step 1: Check if SQL Server LocalDB is Available
```powershell
# Check LocalDB instances
sqllocaldb info

# If MSSQLLocalDB doesn't exist, create it
sqllocaldb create MSSQLLocalDB

# Start the instance
sqllocaldb start MSSQLLocalDB
```

### Step 2: Run the Application
```bash
dotnet run
```

You should see in the console:
```
info: Program[0]
      Database migrated/created successfully
info: Program[0]
      Application starting with OpenTelemetry configured
```

### Step 3: Test the API
Use the `.http` file or cURL:

```bash
# Save a weather record (will go to SQL Server!)
POST https://localhost:7064/weather/record
Content-Type: application/json

{
  "city": "London",
  "temperature": 18,
  "summary": "Mild"
}
```

### Step 4: Verify Data in SQL Server

**Option A: Using Visual Studio**
1. Open **View** ? **SQL Server Object Explorer**
2. Expand **(localdb)\MSSQLLocalDB** ? **Databases** ? **WeatherDb**
3. Right-click **Tables** ? **WeatherRecords** ? **View Data**

**Option B: Using Azure Data Studio / SSMS**
1. Connect to: `(localdb)\MSSQLLocalDB`
2. Run query:
```sql
USE WeatherDb;
SELECT * FROM WeatherRecords;
```

### Step 5: View Traces in Jaeger
1. Open http://localhost:16686
2. Select "JaegerDemo.Api"
3. Click "Find Traces"
4. Click on a trace to see:
   - **db.system**: `mssql` ?
   - **db.statement**: The actual SQL query
   - SQL Server connection and query execution time

## ?? **You Should See:**

### In Application Logs:
```
Activity.TraceId: xxxxxxxxxxxxx
Activity.SpanId: yyyyyyyyyyyy
Resource associated with Activity:
    service.name: JaegerDemo.Api
    service.version: 1.0.0
```

### In Jaeger UI:
```
POST /weather/record
?? POST /weather/record [ASP.NET Core]
?  ?? save-weather-record [Your custom span]
?     ?? INSERT WeatherRecords [EF Core - SQL Server]
?        ?? db.system: mssql
?        ?? db.name: WeatherDb
?        ?? Duration: ~10-20ms
```

### In SQL Server:
```sql
Id | City   | Temperature | Summary | RecordedAt
---|--------|-------------|---------|-------------------------
1  | London | 18          | Mild    | 2025-01-24 16:30:45.123
```

## ?? **Troubleshooting Quick Fixes**

### "Cannot connect to LocalDB"
```powershell
# Stop and restart LocalDB
sqllocaldb stop MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### "Database does not exist"
The app will create it automatically on first run. If issues persist:
```bash
# Manually apply migrations
dotnet ef database update
```

### Want to start fresh?
```bash
# Drop the database
dotnet ef database drop

# Run the app again (will recreate)
dotnet run
```

## ?? **Useful SQL Queries**

### Check Table Structure
```sql
USE WeatherDb;

-- View table schema
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'WeatherRecords';
```

### View Migration History
```sql
USE WeatherDb;
SELECT * FROM __EFMigrationsHistory;
```

### Statistics
```sql
USE WeatherDb;

-- Count by city
SELECT City, COUNT(*) as Count, AVG(Temperature) as AvgTemp
FROM WeatherRecords
GROUP BY City;

-- Latest records
SELECT TOP 10 *
FROM WeatherRecords
ORDER BY RecordedAt DESC;
```

## ? **Success Indicators**

? Application starts without errors  
? Log shows "Database migrated/created successfully"  
? POST requests return 201 Created  
? GET requests return data from SQL Server  
? Jaeger shows `db.system: mssql` in traces  
? SQL Server Object Explorer shows WeatherDb database  
? WeatherRecords table contains your data  

**If all above are ?, you're successfully using SQL Server with OpenTelemetry tracing!** ??
