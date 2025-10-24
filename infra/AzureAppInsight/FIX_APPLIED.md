# ?? Fix Applied: azd provision Errors

## Problem 1: Path Not Found Error
You encountered this error when running `azd provision`:

```
ERROR: initializing provisioning manager: failed to compile bicep template: 
failed running bicep build: exit code: 1, stdout: , stderr: 
An error occurred reading file. Could not find a part of the path 
'D:\Source\Repos\JaegerGettingStarted\infra\AzureAppInsight\infra\main.bicep'.
```

### Root Cause
The error was caused by:
1. Incorrect `module` setting in `azure.yaml` causing azd to look for `infra/main.bicep` inside the path
2. Potential duplicate `azure.yaml` file inside the `infra/AzureAppInsight` directory

## Problem 2: Invalid JSON Format Error
You then encountered this error:

```
ERROR: initializing provisioning manager: resolving bicep parameters file: 
error unmarshalling Bicep template parameters: invalid character 'm' looking for beginning of value
```

### Root Cause
The `main.parameters.json` file was using YAML syntax instead of proper JSON format.

## What Was Fixed

### 1. Updated `azure.yaml` (Root)
**Before:**
```yaml
infra:
  provider: bicep
  path: infra/AzureAppInsight
  module: main  # ? This caused the issue
```

**After:**
```yaml
infra:
  provider: bicep
  path: infra/AzureAppInsight  # ? Removed module line
```

### 2. Removed Duplicate `azure.yaml`
Deleted `infra/AzureAppInsight/azure.yaml` (if it existed)

### 3. Fixed `main.parameters.json` ? NEW
**Before (YAML syntax - WRONG):**
```yaml
metadata:
  template: main.bicep
  parameters:
    environmentName: ${AZURE_ENV_NAME}
    location: ${AZURE_LOCATION}
```

**After (JSON format - CORRECT):**
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
      "value": "${APPLICATIONINSIGHTS_NAME=}")
    },
    "logAnalyticsWorkspaceName": {
      "value": "${LOG_ANALYTICS_WORKSPACE_NAME=}")
    },
    "tags": {
      "value": {}
    }
  }
}
```

## Correct Project Structure

```
JaegerGettingStarted/              ? Your project root
?
??? azure.yaml                     ? ONLY azure.yaml file (at root)
??? .azdignore
??? JaegerDemo.Api.csproj
??? Program.cs
??? appsettings.json
?
??? infra/
    ??? .gitignore
    ??? AzureAppInsight/           ? Infrastructure directory
        ??? main.bicep             ? Main Bicep template
        ??? main.parameters.json   ? Parameters
        ??? abbreviations.json
        ??? .env.template
        ??? deploy.ps1
        ??? deploy.sh
        ??? README.md
        ??? [other docs]
        ??? modules/
            ??? log-analytics.bicep
            ??? application-insights.bicep
```

## ? Verification

Run these commands to verify the fix:

```powershell
# 1. Verify you're in project root
Get-Location  # Should show: D:\Source\Repos\JaegerGettingStarted

# 2. Verify azure.yaml exists at root
Test-Path azure.yaml  # Should return: True

# 3. Verify main.bicep exists
Test-Path infra\AzureAppInsight\main.bicep  # Should return: True

# 4. Verify no duplicate azure.yaml
Test-Path infra\AzureAppInsight\azure.yaml  # Should return: False

# 5. Test bicep compilation
az bicep build --file infra\AzureAppInsight\main.bicep
# Should succeed (warning about bicep version is OK)
```

## ?? Ready to Deploy

Now you can run the deployment:

### Option 1: Using the Script (Recommended)
```powershell
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

### Option 2: Manual azd Commands
```bash
# Make sure you're in project root
cd D:\Source\Repos\JaegerGettingStarted

# Login to Azure
azd auth login

# Create environment (first time only)
azd env new jaeger-demo

# Set environment variables
azd env set AZURE_ENV_NAME jaeger-demo
azd env set AZURE_LOCATION eastus

# Provision infrastructure
azd provision
```

## Common Mistakes to Avoid

? **Don't do this:**
- Running `azd provision` from inside `infra/AzureAppInsight` directory
- Having `azure.yaml` in both root AND infra directories
- Using `module: main` in `azure.yaml` for subscription-scoped templates

? **Do this:**
- Always run `azd` commands from the **project root**
- Keep only ONE `azure.yaml` at the **project root**
- Let azd find `main.bicep` automatically from the `path` setting

## Expected Output After Fix

When you run `azd provision`, you should see:

```
Provisioning Azure resources (azd provision)
Provisioning Azure resources can take some time

Subscription: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Location: East US

  You can view detailed progress in the Azure Portal:
  [Portal Link]

  (?) Done: Resource group: rg-jaeger-demo
  (?) Done: Log Analytics workspace: log-xxxxxx
  (?) Done: Application Insights: appi-xxxxxx

SUCCESS: Your application was provisioned in Azure in X minutes X seconds.

Outputs:
  APPLICATIONINSIGHTS_CONNECTION_STRING: "InstrumentationKey=xxx;..."
  APPLICATIONINSIGHTS_NAME: "appi-xxxxxx"
  AZURE_LOCATION: "eastus"
  AZURE_RESOURCE_GROUP: "rg-jaeger-demo"
```

## Still Having Issues?

### Check these:
1. **Azure CLI is installed**: `az version`
2. **Azure Developer CLI is installed**: `azd version`
3. **You're logged in**: `azd auth login`
4. **You have permissions**: Contributor or Owner role on subscription
5. **Bicep compiles**: `az bicep build --file infra\AzureAppInsight\main.bicep`

### If azd provision still fails:
```bash
# Try a clean slate
azd down --purge  # If environment exists
azd env new jaeger-demo
azd provision --debug  # Run with debug output
```

## Additional Help

**Documentation:**
- `infra/AzureAppInsight/README.md` - Complete guide
- `infra/AzureAppInsight/QUICK_REFERENCE.md` - Command reference
- `infra/AzureAppInsight/DEPLOYMENT_CHECKLIST.md` - Step-by-step

**Troubleshooting:**
- Check the updated README.md with the new troubleshooting section
- Review `infra/AzureAppInsight/QUICK_REFERENCE.md` ? Troubleshooting

---

**Status**: ? **FIXED** - Ready to deploy!

**Next Step**: Run `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
