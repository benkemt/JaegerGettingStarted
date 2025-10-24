# Azure Application Insights Deployment - Quick Reference

## Prerequisites

Install Azure Developer CLI (azd):
```bash
# Windows
winget install microsoft.azd

# macOS
brew tap azure/azd && brew install azd

# Linux
curl -fsSL https://aka.ms/install-azd.sh | bash
```

## Quick Deploy

### Option 1: Using PowerShell Script (Windows/PowerShell)

```powershell
# Navigate to project root
cd D:\Source\Repos\JaegerGettingStarted

# Basic deployment
.\infra\AzureAppInsight\deploy.ps1

# Custom environment and location
.\infra\AzureAppInsight\deploy.ps1 -EnvironmentName "prod" -Location "westus2"

# Automatically update appsettings.json
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings

# All options
.\infra\AzureAppInsight\deploy.ps1 `
    -EnvironmentName "jaeger-demo-prod" `
    -Location "eastus" `
    -SubscriptionId "your-subscription-id" `
    -UpdateAppSettings
```

### Option 2: Using Bash Script (Linux/macOS/WSL)

```bash
# Navigate to project root
cd ~/JaegerGettingStarted

# Make script executable (first time only)
chmod +x infra/AzureAppInsight/deploy.sh

# Basic deployment
./infra/AzureAppInsight/deploy.sh

# Custom environment and location
./infra/AzureAppInsight/deploy.sh -e prod -l westus2

# Automatically update appsettings.json
./infra/AzureAppInsight/deploy.sh -u

# All options
./infra/AzureAppInsight/deploy.sh \
    -e jaeger-demo-prod \
    -l eastus \
    -s your-subscription-id \
    -u
```

### Option 3: Manual azd Commands

```bash
# Login to Azure
azd auth login

# Create environment
azd env new jaeger-demo

# Set variables
azd env set AZURE_ENV_NAME jaeger-demo
azd env set AZURE_LOCATION eastus

# Deploy
azd provision

# Get connection string
azd env get-values | grep APPLICATIONINSIGHTS_CONNECTION_STRING
```

## Common Commands

### View Resources
```bash
# List all environments
azd env list

# Show current environment values
azd env get-values

# Select different environment
azd env select <environment-name>
```

### Update Resources
```bash
# Modify Bicep files, then re-provision
azd provision
```

### Clean Up
```bash
# Delete all Azure resources
azd down

# Delete with local data
azd down --purge
```

## Azure Regions

Popular regions:
- `eastus` - East US (Virginia)
- `eastus2` - East US 2 (Virginia)
- `westus2` - West US 2 (Washington)
- `westeurope` - West Europe (Netherlands)
- `northeurope` - North Europe (Ireland)
- `centralus` - Central US (Iowa)
- `southcentralus` - South Central US (Texas)

## Troubleshooting

### Script not found
```bash
# Ensure you're in the project root
pwd  # Should show JaegerGettingStarted directory

# List files
ls infra/AzureAppInsight/
```

### azd not found
```bash
# Check installation
azd version

# Reinstall if needed
winget install microsoft.azd  # Windows
brew install azd              # macOS
```

### Authentication failed
```bash
# Re-login
azd auth login

# Or use service principal
azd auth login --client-id <id> --client-secret <secret> --tenant-id <tenant>
```

### Provisioning failed
```bash
# Check subscription
az account show

# Set specific subscription
az account set --subscription <subscription-id>
azd provision
```

### Connection string not updating
```bash
# Manually get and copy
azd env get-values | grep CONNECTION_STRING

# Update appsettings.json manually
# Or use dotnet user-secrets

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "<connection-string>"
```

## Cost Estimation

**Free Tier:**
- First 5 GB/month: $0
- Most development apps stay within free tier

**Beyond Free Tier:**
- Data ingestion: ~$2.30/GB
- Data retention (90 days): Included
- Extended retention: $0.10/GB/month

**Set Daily Cap:**
Edit `infra/AzureAppInsight/modules/application-insights.bicep`:
```bicep
Cap: {
  dailyQuotaGb: 1  // Limit to 1 GB/day
}
```

## Environment Variables

The scripts set these automatically:
- `AZURE_ENV_NAME` - Environment identifier
- `AZURE_LOCATION` - Azure region
- `AZURE_SUBSCRIPTION_ID` - Subscription ID (optional)
- `APPLICATIONINSIGHTS_NAME` - Custom resource name (optional)
- `LOG_ANALYTICS_WORKSPACE_NAME` - Custom workspace name (optional)

## Output Variables

After deployment, these are available:
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Full connection string
- `APPLICATIONINSIGHTS_INSTRUMENTATION_KEY` - Instrumentation key
- `APPLICATIONINSIGHTS_NAME` - Resource name
- `AZURE_RESOURCE_GROUP` - Resource group name
- `AZURE_LOCATION` - Deployment location

Access with:
```bash
azd env get-values
```

## Integration with Application

### Option 1: appsettings.json
```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "InstrumentationKey=...;IngestionEndpoint=..."
  }
}
```

### Option 2: User Secrets (Recommended for development)
```bash
dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "<connection-string>"
```

### Option 3: Environment Variable
```bash
# Windows PowerShell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"

# Linux/macOS
export APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

### In Code
```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = 
            builder.Configuration.GetConnectionString("ApplicationInsights") 
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
    });
```

## CI/CD Integration

### GitHub Actions
Add to `.github/workflows/infrastructure.yml`:
```yaml
- name: Deploy Infrastructure
  run: |
    curl -fsSL https://aka.ms/install-azd.sh | bash
    azd provision --no-prompt
  env:
    AZURE_CREDENTIALS: ${{ secrets.AZURE_CREDENTIALS }}
```

### Azure DevOps
Add to `azure-pipelines.yml`:
```yaml
- task: AzureCLI@2
  inputs:
    azureSubscription: 'Azure Subscription'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      curl -fsSL https://aka.ms/install-azd.sh | bash
      azd provision --no-prompt
```

## Next Steps After Deployment

1. **Copy connection string** (automatically done by script or manually)
2. **Update application configuration** (automatic with -u flag)
3. **Run application**: `dotnet run`
4. **Make test requests**: Use JaegerDemo.Api.http file
5. **View in Azure Portal**: Wait 2-3 minutes for data
6. **Set up alerts** for critical failures
7. **Create dashboards** for monitoring

## Support Resources

- **Azure Developer CLI**: https://aka.ms/azd
- **Bicep**: https://aka.ms/bicep
- **Application Insights**: https://aka.ms/appinsights-docs
- **Pricing**: https://azure.microsoft.com/pricing/details/monitor/
