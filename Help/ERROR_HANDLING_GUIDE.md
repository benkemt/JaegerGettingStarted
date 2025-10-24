# Exception Handling & Error Tracing Guide

## ?? **Problem You Identified**
When SQL Server is down, the trace shows an error status but doesn't capture:
- **What** the actual error was
- **Where** it came from (database connection issue)
- **Exception details** (stack trace, error message)

## ? **Solution Applied**

Added `try-catch` blocks with exception recording to all database endpoints:

```csharp
try
{
    // Database operation
    await db.SaveChangesAsync();
    
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    // Record the exception in the activity
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.RecordException(ex);  // THIS IS KEY!
    activity?.AddEvent(new ActivityEvent($"Failed: {ex.Message}"));
    
    logger.LogError(ex, "Operation failed");
    throw;  // Re-throw for proper HTTP error response
}
```

## ?? **How to Test Database Errors**

### Test 1: SQL Server Connection Error

**Step 1: Stop SQL Server LocalDB**
```powershell
sqllocaldb stop MSSQLLocalDB
```

**Step 2: Try to save a weather record**
```bash
POST https://localhost:7064/weather/record
Content-Type: application/json

{
  "city": "London",
  "temperature": 18,
  "summary": "Mild"
}
```

**Step 3: View in Jaeger** (http://localhost:16686)

You'll now see in the trace:

#### ? **Span Details:**
- **Status**: `ERROR`
- **Error**: `true`

#### ? **Exception Tags:**
```
exception.type: Microsoft.Data.SqlClient.SqlException
exception.message: A network-related or instance-specific error occurred while establishing a connection to SQL Server...
exception.stacktrace: [Full stack trace showing the connection error]
```

#### ? **Events Timeline:**
```
1. "Saving weather record for London" (start)
2. "Failed to save weather record: A network-related error..." (error event)
```

#### ? **Span Tags:**
```
city: London
error: true
otel.status_code: ERROR
otel.status_description: A network-related or instance-specific error...
```

### Test 2: Restart SQL Server and Test Success

**Step 1: Start SQL Server LocalDB**
```powershell
sqllocaldb start MSSQLLocalDB
```

**Step 2: Make the same request again**

**Step 3: View in Jaeger**

You'll see:
- ? **Status**: `OK`
- ? **No error tags**
- ? **Event**: "Weather record saved with ID: 1"
- ? **Tag**: `record.id: 1`

## ?? **What You'll See in Jaeger UI**

### When SQL Server is DOWN ?

```
POST /weather/record [RED - ERROR]
?? Status: ERROR
?? Duration: ~100ms (timeout/retry attempts)
?? save-weather-record [RED - ERROR]
   ?? Tags:
   ?  ?? city: London
   ?  ?? error: true
   ?  ?? otel.status_code: ERROR
   ?? Events:
   ?  ?? "Saving weather record for London"
   ?  ?? "Failed to save weather record: A network-related..."
   ?? Exception:
      ?? exception.type: Microsoft.Data.SqlClient.SqlException
      ?? exception.message: A network-related or instance-specific error...
      ?? exception.stacktrace: 
         at Microsoft.Data.SqlClient.SqlConnection.Open()
         at Microsoft.EntityFrameworkCore...
```

### When SQL Server is UP ?

```
POST /weather/record [GREEN - OK]
?? Status: OK
?? Duration: ~15ms
?? save-weather-record [GREEN - OK]
   ?? Tags:
   ?  ?? city: London
   ?  ?? record.id: 1
   ?  ?? otel.status_code: OK
   ?? Events:
   ?  ?? "Saving weather record for London"
   ?  ?? "Weather record saved with ID: 1"
   ?? Child Span: INSERT WeatherRecords (EF Core)
      ?? db.system: mssql
      ?? db.statement: INSERT INTO [WeatherRecords]...
      ?? Duration: 8ms
```

## ?? **Exception Details You'll See**

### Database Connection Error
```
exception.type: Microsoft.Data.SqlClient.SqlException
exception.message: A network-related or instance-specific error occurred 
                   while establishing a connection to SQL Server. 
                   The server was not found or was not accessible.
```

### Database Timeout Error
```
exception.type: Microsoft.Data.SqlClient.SqlException
exception.message: Timeout expired. The timeout period elapsed prior to 
                   completion of the operation or the server is not responding.
```

### Constraint Violation
```
exception.type: Microsoft.EntityFrameworkCore.DbUpdateException
exception.message: An error occurred while saving the entity changes.
                   Inner exception: Cannot insert duplicate key...
```

## ?? **Key Benefits**

| Without Exception Recording | With Exception Recording |
|-----------------------------|-------------------------|
| ? Just see "Error" status | ? See exact error type |
| ? No error message | ? Full error message |
| ? No stack trace | ? Complete stack trace |
| ? Can't tell if DB is down | ? Clear: "Connection failed" |
| ? Hard to debug | ? Easy to diagnose |

## ?? **Testing Checklist**

### ? Test Different Error Scenarios:

1. **SQL Server Down**
   ```powershell
   sqllocaldb stop MSSQLLocalDB
   ```
   - Make POST request
   - Check Jaeger for connection error details

2. **SQL Server Up (Normal Operation)**
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```
   - Make POST request
   - Check Jaeger for success status

3. **Database Lock/Timeout**
   - Start a long-running query in SQL Server
   - Make request while it's running
   - Check Jaeger for timeout error

4. **Invalid Data**
   - Try inserting a record that violates constraints
   - Check Jaeger for constraint violation details

## ?? **Observability Best Practices**

### ? DO:
- Always wrap database operations in try-catch
- Use `activity?.RecordException(ex)` for all exceptions
- Set `ActivityStatusCode.Error` when errors occur
- Add descriptive events before/after operations
- Log errors with `logger.LogError(ex, ...)`

### ? DON'T:
- Swallow exceptions without recording them
- Only set error status without recording the exception
- Forget to re-throw after recording (unless handling gracefully)

## ?? **Real-World Debugging Example**

**Scenario**: Production API suddenly starts failing

**Without Exception Tracing**:
```
Developer: "The API is returning 500 errors"
Ops: "Let me check the logs... nothing obvious"
Developer: "Let me check Jaeger... just see ERROR status"
Result: 30 minutes of investigation, unclear root cause
```

**With Exception Tracing**:
```
Developer: "The API is returning 500 errors"
Ops: "Let me check Jaeger..."
Jaeger shows: exception.message: "Login failed for user 'appuser'"
Result: 2 minutes to identify password expired issue
```

## ? **Summary**

Now when you stop SQL Server and make a request, you'll see in Jaeger:
- ? **Red error indicator** on the span
- ? **Exception type**: SqlException
- ? **Error message**: "Server was not found or was not accessible"
- ? **Full stack trace** pinpointing the connection attempt
- ? **Event timeline** showing where it failed

This makes debugging database issues **10x faster**! ??
