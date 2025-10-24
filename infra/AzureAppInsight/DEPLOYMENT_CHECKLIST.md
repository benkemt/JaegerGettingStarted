# Azure Deployment Checklist

Use this checklist when deploying Application Insights to Azure.

## Pre-Deployment Checklist

### Prerequisites
- [ ] Azure Developer CLI (azd) installed
  ```bash
  azd version  # Should show version number
  ```
- [ ] Azure CLI installed (optional but recommended)
  ```bash
  az version
  ```
- [ ] Azure subscription with active credits
- [ ] Contributor or Owner role on subscription
- [ ] PowerShell 7+ or Bash shell

### Project Setup
- [ ] Current directory is project root (`JaegerGettingStarted`)
- [ ] `infra/AzureAppInsight` folder exists
- [ ] Bicep files are present in `infra/AzureAppInsight/modules`
- [ ] `azure.yaml` exists in project root

## Deployment Checklist

### Step 1: Choose Your Deployment Method

Pick ONE method:

**Option A: Automated Script (Recommended for beginners)**
- [ ] Windows PowerShell: `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
- [ ] Linux/macOS: `./infra/AzureAppInsight/deploy.sh -u`

**Option B: Manual azd Commands (More control)**
- [ ] `azd auth login`
- [ ] `azd env new <environment-name>`
- [ ] `azd env set AZURE_ENV_NAME <name>`
- [ ] `azd env set AZURE_LOCATION <region>`
- [ ] `azd provision`

### Step 2: During Deployment

- [ ] Browser opens for Azure authentication
- [ ] Sign in with Azure credentials
- [ ] Grant permissions if prompted
- [ ] Wait for deployment to complete (2-5 minutes)
- [ ] Note any errors in terminal output

### Step 3: Verify Deployment

- [ ] Deployment completed successfully (no errors)
- [ ] Connection string displayed in terminal
- [ ] Connection string copied to clipboard (if available)
- [ ] `appsettings.json` updated (if using `-UpdateAppSettings`)

### Step 4: Azure Portal Verification

- [ ] Navigate to Azure Portal (https://portal.azure.com)
- [ ] Find resource group (e.g., `rg-jaeger-demo`)
- [ ] Verify 3 resources exist:
  - [ ] Resource Group
  - [ ] Log Analytics Workspace (log-xxxxx)
  - [ ] Application Insights (appi-xxxxx)
- [ ] Open Application Insights resource
- [ ] Copy connection string from Overview page

## Application Configuration Checklist

### Update Configuration

Choose ONE method:

**Method 1: appsettings.json (Simple, not recommended for production)**
- [ ] Open `appsettings.json`
- [ ] Find `ConnectionStrings` section
- [ ] Update `ApplicationInsights` value with connection string
- [ ] Save file

**Method 2: User Secrets (Recommended for development)**
```bash
dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "YOUR_CONNECTION_STRING"
```
- [ ] Command executed successfully
- [ ] Secret stored securely

**Method 3: Environment Variable (Alternative)**
```powershell
# Windows PowerShell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING = "YOUR_CONNECTION_STRING"
```
```bash
# Linux/macOS
export APPLICATIONINSIGHTS_CONNECTION_STRING="YOUR_CONNECTION_STRING"
```
- [ ] Environment variable set

### Verify Configuration in Code

- [ ] Open `Program.cs`
- [ ] Verify `UseAzureMonitor()` is called
- [ ] Verify connection string is read from configuration
- [ ] `Azure.Monitor.OpenTelemetry.AspNetCore` package is installed

## Testing Checklist

### Local Testing

- [ ] Build application: `dotnet build`
- [ ] Run application: `dotnet run`
- [ ] Application starts without errors
- [ ] Navigate to `https://localhost:7064/weatherforecast`
- [ ] API returns weather data
- [ ] No OpenTelemetry errors in console

### Verify Jaeger (Local)

- [ ] Open http://localhost:16686
- [ ] Select "JaegerDemo.Api" service
- [ ] Click "Find Traces"
- [ ] See recent traces
- [ ] Expand a trace and verify spans

### Verify Application Insights (Azure)

- [ ] Wait 2-3 minutes after making requests
- [ ] Open Azure Portal
- [ ] Navigate to Application Insights resource
- [ ] Click "Transaction search"
- [ ] Set time range to "Last 30 minutes"
- [ ] See your API requests listed
- [ ] Click on a request to see details
- [ ] Verify spans and timings match Jaeger

## Advanced Verification Checklist

### Test Error Tracking

- [ ] Make request to `/weather/Tokyo` (triggers exception)
- [ ] Check Jaeger - see error status on span
- [ ] Wait 2-3 minutes
- [ ] Check Azure Portal ? Failures
- [ ] See `InvalidOperationException` listed
- [ ] Click on exception to see stack trace

### Test Database Tracing

- [ ] Make POST request to `/weather/record`
- [ ] Check Jaeger - see SQL query span
- [ ] Wait 2-3 minutes
- [ ] Check Azure Portal ? Dependencies
- [ ] Filter by type: SQL
- [ ] See database operation listed
- [ ] Verify query execution time

### Test Application Map

- [ ] Azure Portal ? Application Insights
- [ ] Click "Application Map"
- [ ] See your API component
- [ ] See SQL Database dependency
- [ ] Verify connection lines between components
- [ ] Check success rate and response times

## Production Deployment Checklist

### Before Going to Production

- [ ] Remove connection string from `appsettings.json`
- [ ] Configure connection string in:
  - [ ] Azure App Service: Application Settings
  - [ ] Azure Container Apps: Secrets
  - [ ] Azure Kubernetes: Kubernetes Secrets
  - [ ] Azure Key Vault (recommended)
- [ ] Enable sampling for high-volume apps
  ```csharp
  .SetSampler(new TraceIdRatioBasedSampler(0.1)) // 10% sampling
  ```
- [ ] Set daily data cap in Azure Portal
  - [ ] Application Insights ? Usage and estimated costs
  - [ ] Daily cap ? Set value (e.g., 1 GB)
- [ ] Configure alerts
  - [ ] Failed requests > threshold
  - [ ] Response time > threshold
  - [ ] Exception count > threshold
- [ ] Set up dashboard for monitoring
- [ ] Configure retention policy (default: 90 days)

### Production Environment

- [ ] Create separate environment: `azd env new production`
- [ ] Use different resource names (avoid conflicts)
- [ ] Deploy production infrastructure: `azd provision`
- [ ] Update production app configuration
- [ ] Test production deployment
- [ ] Monitor data ingestion costs

## Cost Management Checklist

### Monitor Usage

- [ ] Azure Portal ? Application Insights
- [ ] Navigate to "Usage and estimated costs"
- [ ] Check daily data ingestion volume
- [ ] Verify staying within free tier (5 GB/month)
- [ ] Set up budget alerts if approaching limits

### Optimize Costs

- [ ] Enable sampling if ingesting > 1 GB/day
- [ ] Set daily cap to prevent overages
- [ ] Review retention settings (90 days default)
- [ ] Remove unnecessary instrumentation
- [ ] Filter out noisy telemetry

## Troubleshooting Checklist

### No Data in Azure?

- [ ] Wait 2-3 minutes (ingestion delay)
- [ ] Check connection string is correct
- [ ] Verify no typos in connection string
- [ ] Check application logs for Azure Monitor errors
- [ ] Verify firewall allows HTTPS to *.applicationinsights.azure.com
- [ ] Test network connectivity: `ping eastus-1.in.applicationinsights.azure.com`

### Deployment Failed?

- [ ] Check Azure subscription is active
- [ ] Verify you have Contributor/Owner role
- [ ] Try different Azure region
- [ ] Check resource name conflicts
- [ ] Review error message in terminal
- [ ] Run `azd down` and retry `azd provision`

### Build Errors?

- [ ] Verify .NET 9 SDK installed: `dotnet --version`
- [ ] Restore packages: `dotnet restore`
- [ ] Check `Azure.Monitor.OpenTelemetry.AspNetCore` package is installed
- [ ] Verify `using Azure.Monitor.OpenTelemetry.AspNetCore;` in Program.cs
- [ ] Clean and rebuild: `dotnet clean && dotnet build`

## Cleanup Checklist

### When Finished Testing

- [ ] Stop application (Ctrl+C)
- [ ] Stop Jaeger: `docker-compose down`
- [ ] Delete Azure resources: `azd down`
- [ ] Confirm deletion in terminal
- [ ] Verify resources deleted in Azure Portal

### Complete Cleanup

- [ ] Remove `.azure` folder (local environment data)
- [ ] Remove connection string from `appsettings.json`
- [ ] Clear user secrets: `dotnet user-secrets clear`
- [ ] Unset environment variables

## Documentation Reference

When you need help, refer to these documents:

| Issue | Document |
|-------|----------|
| First-time deployment | `infra/AzureAppInsight/README.md` |
| Quick commands | `infra/AzureAppInsight/QUICK_REFERENCE.md` |
| Architecture questions | `infra/AzureAppInsight/ARCHITECTURE_OVERVIEW.md` |
| Application integration | `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` |
| Deployment overview | `Help/AZURE_DEPLOYMENT_SUMMARY.md` |

## Success Criteria

You've successfully deployed when:

- ? Infrastructure deployed to Azure without errors
- ? Application Insights resource created
- ? Connection string obtained
- ? Application configuration updated
- ? Application runs without errors
- ? Traces visible in both Jaeger and Azure Portal
- ? Database operations traced correctly
- ? Exceptions captured and visible in Azure
- ? Application Map shows component dependencies

## Next Steps After Successful Deployment

1. [ ] Explore Azure Portal features (Transaction search, Performance, Failures)
2. [ ] Set up alerts for critical errors
3. [ ] Create custom dashboard
4. [ ] Learn KQL for advanced queries
5. [ ] Deploy to production environment
6. [ ] Integrate with CI/CD pipeline
7. [ ] Share with team members

---

**Need Help?**
- Check the troubleshooting section in `infra/AzureAppInsight/QUICK_REFERENCE.md`
- Review error messages carefully
- Ensure all prerequisites are met
- Try redeployment: `azd down` then `azd provision`
