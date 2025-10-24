# ? All Fixes Applied - Ready to Deploy!

## Summary of Errors and Fixes

You encountered **two errors** when running `azd provision`. Both have been fixed!

---

## ? Error 1: Path Not Found

### What You Saw
```
ERROR: initializing provisioning manager: failed to compile bicep template: 
failed running bicep build: exit code: 1
An error occurred reading file. Could not find a part of the path 
'D:\Source\Repos\JaegerGettingStarted\infra\AzureAppInsight\infra\main.bicep'.
```

### Why It Happened
- `azure.yaml` had `module: main` which caused azd to look for the wrong path
- azd was looking for: `infra/AzureAppInsight/infra/main.bicep` ?
- Actual location is: `infra/AzureAppInsight/main.bicep` ?

### ? Fix Applied
**File: `azure.yaml`**
```yaml
# Removed the "module: main" line
infra:
  provider: bicep
  path: infra/AzureAppInsight
```

---

## ? Error 2: Invalid JSON Format

### What You Saw
```
ERROR: initializing provisioning manager: resolving bicep parameters file: 
error unmarshalling Bicep template parameters: 
invalid character 'm' looking for beginning of value
```

### Why It Happened
- `main.parameters.json` was using **YAML syntax** instead of **JSON format**
- Azure Resource Manager expects valid JSON for parameter files

### ? Fix Applied
**File: `infra/AzureAppInsight/main.parameters.json`**

Converted from YAML to proper JSON:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environmentName": {
      "value": "${AZURE_ENV_NAME}"
    },
    "location": {
      "value": "${AZURE_LOCATION}"
    },
    "applicationInsightsName": {
      "value": "${APPLICATIONINSIGHTS_NAME=}"
    },
    "logAnalyticsWorkspaceName": {
      "value": "${LOG_ANALYTICS_WORKSPACE_NAME=}"
    },
    "tags": {
      "value": {}
    }
  }
}
```

---

## ? Verification Results

All checks passed! ?

| Check | Status | Result |
|-------|--------|--------|
| Project root location | ? | `D:\Source\Repos\JaegerGettingStarted` |
| `azure.yaml` at root | ? | Exists and correct |
| `main.bicep` exists | ? | Found at `infra/AzureAppInsight/main.bicep` |
| Bicep compilation | ? | Compiles successfully |
| JSON validation | ? | `main.parameters.json` is valid JSON |
| .NET build | ? | Build successful |

---

## ?? Ready to Deploy!

### Quick Deploy (Recommended)

```powershell
# From project root: D:\Source\Repos\JaegerGettingStarted
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

### Manual Deployment Steps

```bash
# 1. Ensure you're in project root
cd D:\Source\Repos\JaegerGettingStarted

# 2. Login to Azure (if not already logged in)
azd auth login

# 3. Create environment (first time only)
azd env new jaeger-demo

# 4. Set environment variables
azd env set AZURE_ENV_NAME jaeger-demo
azd env set AZURE_LOCATION eastus

# 5. Provision infrastructure
azd provision
```

---

## ?? Expected Deployment Flow

When you run `azd provision`, you should see:

```
Provisioning Azure resources (azd provision)
Provisioning Azure resources can take some time.

? Select an Azure Subscription to use:
  [Select your subscription]

Subscription: Visual Studio Enterprise 2025-2028-1 (d716cc3f-...)
Location: East US

  You can view detailed progress in the Azure Portal:
  https://portal.azure.com/#blade/...

  (?) Done: Resource group: rg-jaeger-demo
  (?) Done: Log Analytics workspace: log-xxxxxx  
  (?) Done: Application Insights: appi-xxxxxx

SUCCESS: Your application was provisioned in Azure in 3 minutes.

Outputs:
  APPLICATIONINSIGHTS_CONNECTION_STRING: "InstrumentationKey=xxx;..."
  APPLICATIONINSIGHTS_INSTRUMENTATION_KEY: "xxxxxxxx-xxxx-xxxx-xxxx-xxx"
  APPLICATIONINSIGHTS_NAME: "appi-xxxxxx"
  AZURE_LOCATION: "eastus"
  AZURE_RESOURCE_GROUP: "rg-jaeger-demo"
```

---

## ?? What Gets Deployed

1. **Resource Group**: `rg-jaeger-demo`
   - Container for all resources
   
2. **Log Analytics Workspace**: `log-xxxxxx`
   - Data storage and analytics backend
   - 30-day retention (default)
   - PerGB2018 pricing tier
   
3. **Application Insights**: `appi-xxxxxx`
   - APM and distributed tracing
   - 90-day retention (default)
   - OpenTelemetry compatible

**Estimated Time**: 3-5 minutes

**Estimated Cost**: $0 (within free tier for typical dev usage)

---

## ?? After Deployment

### 1. Get Your Connection String

The connection string will be displayed in the output. You can also retrieve it:

```bash
azd env get-values | grep APPLICATIONINSIGHTS_CONNECTION_STRING
```

### 2. Update Your Application

#### Option A: Manual Update
Copy the connection string and update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "YOUR_CONNECTION_STRING_HERE"
  }
}
```

#### Option B: User Secrets (Recommended)
```bash
dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "YOUR_CONNECTION_STRING"
```

#### Option C: Environment Variable
```powershell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING = "YOUR_CONNECTION_STRING"
```

### 3. Run Your Application

```bash
dotnet run
```

### 4. Make Test Requests

```bash
# Weather forecast
curl https://localhost:7064/weatherforecast -k

# Save to database
curl -X POST https://localhost:7064/weather/record -k `
  -H "Content-Type: application/json" `
  -d '{"city":"London","temperature":18,"summary":"Mild"}'
```

### 5. View Traces

**Jaeger (Instant):**
- http://localhost:16686
- Service: "JaegerDemo.Api"
- Click "Find Traces"

**Azure Portal (2-3 min delay):**
- https://portal.azure.com
- Navigate to Application Insights resource
- Click "Transaction search"
- View your traces!

---

## ?? Troubleshooting

### If deployment still fails:

#### Check Prerequisites
```bash
# Azure CLI
az version

# Azure Developer CLI  
azd version

# You're logged in
azd auth login --check-status
```

#### Verify File Structure
```powershell
# All should return True
Test-Path azure.yaml
Test-Path infra\AzureAppInsight\main.bicep
Test-Path infra\AzureAppInsight\main.parameters.json

# Should return False (no duplicate)
Test-Path infra\AzureAppInsight\azure.yaml
```

#### Test Bicep Compilation
```bash
az bicep build --file infra\AzureAppInsight\main.bicep
# Should succeed (warning about version is OK)
```

#### Validate JSON
```powershell
Get-Content infra\AzureAppInsight\main.parameters.json | ConvertFrom-Json
# Should parse without errors
```

### Common Issues

| Issue | Solution |
|-------|----------|
| "Subscription not found" | Run `azd auth login` again |
| "Insufficient permissions" | Need Contributor/Owner role |
| "Resource name conflict" | Change `AZURE_ENV_NAME` to something unique |
| "Quota exceeded" | Check your subscription limits |

---

## ?? Additional Resources

### Documentation
- **Complete Guide**: `infra/AzureAppInsight/README.md`
- **Quick Reference**: `infra/AzureAppInsight/QUICK_REFERENCE.md`
- **Architecture**: `infra/AzureAppInsight/ARCHITECTURE_OVERVIEW.md`
- **Checklist**: `infra/AzureAppInsight/DEPLOYMENT_CHECKLIST.md`
- **Integration**: `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md`

### Support
- **Azure Portal**: Support + Troubleshooting
- **Azure Developer CLI**: https://github.com/Azure/azure-dev
- **Bicep**: https://github.com/Azure/bicep

---

## ?? Summary

### What Was Broken
1. ? Wrong path in `azure.yaml` (had `module: main`)
2. ? Invalid JSON in `main.parameters.json` (was YAML format)

### What Got Fixed
1. ? Removed `module: main` from `azure.yaml`
2. ? Converted `main.parameters.json` to proper JSON format
3. ? Removed any duplicate `azure.yaml` files

### What's Working Now
- ? Bicep templates compile
- ? Parameters file is valid JSON
- ? .NET project builds
- ? Ready for deployment

### Next Steps
1. Run `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
2. Wait 3-5 minutes for deployment
3. Copy connection string
4. Run your application
5. View traces in Azure Portal!

---

**Status**: ?? **ALL ISSUES RESOLVED** - Ready to deploy! ??

**Last Updated**: After fixing JSON format issue

**Next Command**: `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
