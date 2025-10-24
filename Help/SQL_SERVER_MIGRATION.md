# SQL Server Migration Guide

## ? **Changes Made**

### 1. **NuGet Packages**
- ? Removed: `Microsoft.EntityFrameworkCore.Sqlite`
- ? Added: `Microsoft.EntityFrameworkCore.SqlServer` (9.0.10)
- ? Added: `Microsoft.EntityFrameworkCore.Design` (9.0.10) - For migrations

### 2. **Connection String**
Added to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "WeatherDb": "Server=(localdb)\\mssqllocaldb;Database=WeatherDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. **DbContext Configuration**
Changed in `Program.cs`:
```csharp
// OLD (SQLite):
options.UseSqlite("Data Source=weather.db")

// NEW (SQL Server):
options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherDb"))
```

### 4. **Database Initialization**
Changed from `EnsureCreated()` to `Migrate()`:
```csharp
// OLD:
db.Database.EnsureCreated();

// NEW (with migrations):
db.Database.Migrate();
```

### 5. **Initial Migration Created**
- Migration file: `Migrations/XXXXXX_InitialCreate.cs`
- Creates `WeatherRecords` table with proper SQL Server schema

## ??? **Connection String Options**

### Option 1: LocalDB (Current - Development)
```json
"Server=(localdb)\\mssqllocaldb;Database=WeatherDb;Trusted_Connection=True;TrustServerCertificate=True"
```
- **Best for**: Local development
- **Requires**: SQL Server LocalDB (included with Visual Studio)
- **Location**: User profile folder

### Option 2: SQL Server Express
```json
"Server=.\\SQLEXPRESS;Database=WeatherDb;Trusted_Connection=True;TrustServerCertificate=True"
```
- **Best for**: Local SQL Server Express instance
- **Requires**: SQL Server Express installed

### Option 3: Full SQL Server with Authentication
```json
"Server=localhost;Database=WeatherDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```
- **Best for**: Full SQL Server instance
- **Requires**: SQL Server credentials

### Option 4: Azure SQL Database
```json
"Server=tcp:yourserver.database.windows.net,1433;Database=WeatherDb;User Id=yourusername;Password=yourpassword;Encrypt=True"
```
- **Best for**: Production/Cloud deployment

## ?? **How to Use**

### 1. **Verify SQL Server LocalDB is installed**
```powershell
sqllocaldb info
```

If not installed, it comes with:
- Visual Studio (any edition)
- SQL Server Express with Advanced Services

### 2. **Start the Application**
```bash
dotnet run
```

The database will be automatically created on first run using the migration.

### 3. **View the Database**
You can connect to the database using:
- **SQL Server Object Explorer** in Visual Studio
- **Azure Data Studio**
- **SQL Server Management Studio (SSMS)**

Connection string: `(localdb)\mssqllocaldb`

### 4. **Query the Database Directly**
```sql
USE WeatherDb;

-- View all weather records
SELECT * FROM WeatherRecords ORDER BY RecordedAt DESC;

-- Get records by city
SELECT * FROM WeatherRecords WHERE City = 'London';

-- Count records per city
SELECT City, COUNT(*) as RecordCount 
FROM WeatherRecords 
GROUP BY City;
```

## ?? **Database Schema**

The `WeatherRecords` table structure:

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PRIMARY KEY, IDENTITY(1,1) |
| City | nvarchar(max) | NOT NULL |
| Temperature | int | NOT NULL |
| Summary | nvarchar(max) | NOT NULL |
| RecordedAt | datetime2(7) | NOT NULL |

## ?? **EF Core Migrations Commands**

### Create a New Migration
```bash
dotnet ef migrations add MigrationName
```

### Apply Migrations
```bash
dotnet ef database update
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove
```

### View Migrations Status
```bash
dotnet ef migrations list
```

### Generate SQL Script
```bash
dotnet ef migrations script
```

### Drop Database (be careful!)
```bash
dotnet ef database drop
```

## ?? **What You'll See in Jaeger**

Database operations will now show:
- **db.system**: `mssql` (instead of `sqlite`)
- **db.name**: `WeatherDb`
- **db.connection_string**: Server connection info
- **SQL statements**: Actual T-SQL queries sent to SQL Server

Example trace:
```
POST /weather/record
?? save-weather-record
   ?? INSERT INTO [WeatherRecords] (EF Core)
      ?? db.system: mssql
      ?? db.name: WeatherDb
      ?? db.statement: INSERT INTO [WeatherRecords]...
      ?? Duration: 12ms
```

## ?? **Benefits of SQL Server vs SQLite**

| Feature | SQLite | SQL Server |
|---------|--------|------------|
| Concurrent writes | ? Limited | ? Excellent |
| Scalability | ? Small datasets | ? Enterprise-scale |
| Advanced features | ? Basic | ? Full T-SQL, procedures, etc. |
| Transactions | ? Basic | ? Advanced (ACID) |
| Azure integration | ? No | ? Azure SQL Database |
| Production ready | ? Not recommended | ? Yes |

## ?? **Troubleshooting**

### Error: "LocalDB is not installed"
**Solution**: Install SQL Server Express or use Visual Studio installer to add LocalDB

### Error: "Cannot open database"
**Solution**: Check connection string and ensure SQL Server service is running
```powershell
sqllocaldb start mssqllocaldb
```

### Error: "Login failed for user"
**Solution**: Use Trusted_Connection=True or verify SQL Server authentication

### Error: "Certificate validation failed"
**Solution**: Add `TrustServerCertificate=True` to connection string

## ?? **Environment-Specific Configuration**

Create additional configuration files:

### `appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "WeatherDb": "Server=(localdb)\\mssqllocaldb;Database=WeatherDb_Dev;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### `appsettings.Production.json`
```json
{
  "ConnectionStrings": {
    "WeatherDb": "Server=prod-server;Database=WeatherDb;User Id=appuser;Password=SecurePassword;TrustServerCertificate=True"
  }
}
```

## ?? **Security Best Practices**

1. **Never commit passwords** - Use User Secrets or Azure Key Vault
2. **Use connection string encryption** in production
3. **Principle of least privilege** - Create dedicated database user
4. **Enable SSL/TLS** - Use `Encrypt=True` for production

### Using User Secrets (Development)
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:WeatherDb" "your-connection-string"
```

## ? **Summary**

Your application now uses **SQL Server** instead of SQLite:
- ? Production-ready database
- ? Better concurrency and scalability
- ? Full T-SQL support
- ? EF Core Migrations for schema management
- ? OpenTelemetry tracing for SQL Server queries
- ? Ready for Azure SQL Database deployment

The database will be automatically created and migrated on application startup! ??
