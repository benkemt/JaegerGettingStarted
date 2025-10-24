# Deploying Azure Application Insights with Azure Developer CLI (azd)

This directory contains infrastructure-as-code (IaC) using Bicep templates to deploy Azure Application Insights with Azure Developer CLI (azd).

## Prerequisites

1. **Install Azure Developer CLI (azd)**
   ```bash
   # Windows (PowerShell)
   winget install microsoft.azd
   
   # macOS
   brew tap azure/azd && brew install azd
   
   # Linux
   curl -fsSL https://aka.ms/install-azd.sh | bash
   ```

2. **Install Azure CLI** (if not already installed)
   ```bash
   winget install -e --id Microsoft.AzureCLI
   ```

3. **Azure Subscription**: You need an active Azure subscription

## Project Structure

```
infra/AzureAppInsight/
├── main.bicep                      # Main infrastructure template
├── main.parameters.json            # Parameters configuration
├── abbreviations.json              # Azure resource naming conventions
├── modules/
│   ├── application-insights.bicep  # Application Insights module
│   └── log-analytics.bicep         # Log Analytics workspace module
└── .env.template                   # Environment variables template
```

## Quick Start

### 1. Ensure You're in the Project Root

Navigate to the root of your project (where `azure.yaml` is located):

```bash
cd D:\Source\Repos\JaegerGettingStarted
# Or on Linux/macOS
cd ~/JaegerGettingStarted
```

**Important**: The `azure.yaml` file must be in the **project root**, not inside the `infra` folder.

### 2. Initialize Environment (First Time Only)

If you haven't already initialized azd:

```bash
azd init
```

This will create a `.azure` folder with environment-specific configuration.

**Note**: If azd asks you to select a template, choose "Use code in the current directory".

### 3. Set Environment Variables

Copy the `.env.template` to create your environment file:

```bash
# Create .env file from template
cp .env.template .env
```

Edit `.env` and set your values:

```bash
AZURE_ENV_NAME="jaeger-demo"           # Your environment name (must be unique)
AZURE_LOCATION="eastus"                # Azure region (e.g., eastus, westus2, westeurope)
AZURE_SUBSCRIPTION_ID="your-sub-id"    # Your Azure subscription ID (optional)
```

### 4. Login to Azure

```bash
azd auth login
```

This will open a browser window for authentication.

### 5. Provision Infrastructure

```bash
azd provision
```

This command will:
- Create a resource group
- Deploy Log Analytics workspace
- Deploy Application Insights resource
- Output the connection string

**Expected output:**
```
Provisioning Azure resources (azd provision)
Provisioning Azure resources can take some time

Subscription: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Location: East US

  You can view detailed progress in the Azure Portal:
  https://portal.azure.com/#blade/HubsExtension/DeploymentDetailsBlade/...

  (✓) Done: Resource group: rg-jaeger-demo
  (✓) Done: Log Analytics workspace: log-xxxxxx
  (✓) Done: Application Insights: appi-xxxxxx

SUCCESS: Your application was provisioned in Azure in X minutes X seconds.
You can view the resources created under the resource group rg-jaeger-demo in Azure Portal:
https://portal.azure.com/#@/resource/subscriptions/xxx/resourceGroups/rg-jaeger-demo

Outputs:
  APPLICATIONINSIGHTS_CONNECTION_STRING: "InstrumentationKey=xxx;IngestionEndpoint=https://..."
  APPLICATIONINSIGHTS_INSTRUMENTATION_KEY: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  APPLICATIONINSIGHTS_NAME: "appi-xxxxxx"
  AZURE_LOCATION: "eastus"
  AZURE_RESOURCE_GROUP: "rg-jaeger-demo"
```

### 6. Get the Connection String

After deployment, retrieve the connection string:

```bash
azd env get-values
```

Or view specific output:

```bash
azd env get-values | grep APPLICATIONINSIGHTS_CONNECTION_STRING
```

### 7. Update Your Application

Copy the connection string and update your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "YOUR_CONNECTION_STRING_HERE"
  }
}
```

**Better approach - Use environment variables:**

```bash
azd env set APPLICATIONINSIGHTS_CONNECTION_STRING "your-connection-string"
```

Then in your app, read from environment:

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights") 
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
    });
```

## Advanced Usage

### Customize Resource Names

Set environment variables before provisioning:

```bash
azd env set APPLICATIONINSIGHTS_NAME "my-custom-appinsights"
azd env set LOG_ANALYTICS_WORKSPACE_NAME "my-custom-loganalytics"
azd provision
```

### Deploy to Different Environment

```bash
# Create a new environment
azd env new production

# Set production values
azd env set AZURE_ENV_NAME "jaeger-demo-prod"
azd env set AZURE_LOCATION "westus2"

# Provision production environment
azd provision
```

### View Deployment History

```bash
azd provision --preview  # Preview changes before deployment
```

### Clean Up Resources

To delete all Azure resources:

```bash
azd down
```

This will:
- Delete the resource group
- Remove all resources (Application Insights, Log Analytics)
- Keep local configuration (unless you use `--purge`)

## Bicep Modules Explained

### main.bicep
The main entry point that:
- Creates a resource group
- Deploys Log Analytics workspace
- Deploys Application Insights
- Returns connection strings as outputs

### modules/log-analytics.bicep
Deploys a Log Analytics workspace with:
- Configurable SKU (default: PerGB2018)
- Retention period (default: 30 days)
- Public network access enabled

### modules/application-insights.bicep
Deploys Application Insights with:
- Integration with Log Analytics workspace
- Configurable sampling (default: 100%)
- Data retention (default: 90 days)
- Connection string output

## Cost Management

### Estimated Costs

**Log Analytics Workspace:**
- First 5 GB/day: Free
- Additional data: ~$2.30/GB
- Data retention (30 days): Free
- Extended retention: $0.10/GB/month

**Application Insights:**
- First 5 GB/month: Free
- Additional data: ~$2.30/GB
- Data retention (90 days): Included
- Extended retention: $0.10/GB/month

**Typical small app usage:** Often within free tier!

### Monitor Usage

1. Go to Azure Portal → Your Application Insights
2. Navigate to **Usage and estimated costs**
3. View daily ingestion and costs

### Set Daily Cap

Add to `application-insights.bicep`:

```bicep
Cap: {
  dailyQuotaGb: 1  // Limit to 1 GB per day
}
```

## Troubleshooting

### Error: "Could not find a part of the path 'infra/AzureAppInsight/infra/main.bicep'"

**Cause**: The `azure.yaml` file has incorrect path configuration or is in the wrong location.

**Solution**:
1. Ensure `azure.yaml` is in the **project root** (where your `.csproj` file is), NOT inside the `infra` folder
2. Verify the `infra.path` in `azure.yaml` points to `infra/AzureAppInsight`
3. Make sure there's no duplicate `azure.yaml` file inside `infra/AzureAppInsight`
4. Run `azd provision` from the project root directory

**Correct structure**:
```
JaegerGettingStarted/              ← Run azd commands here
├── azure.yaml                     ← azure.yaml at root
├── JaegerDemo.Api.csproj
├── Program.cs
└── infra/
    └── AzureAppInsight/
        ├── main.bicep             ← Bicep template here
        ├── main.parameters.json
        └── modules/
```

### azd command not found
Ensure Azure Developer CLI is installed and in your PATH:
```bash
azd version
```

### Insufficient permissions
Ensure you have:
- Contributor or Owner role on the subscription
- Permission to create resource groups

### Resource name conflicts
If resource names are taken, set custom names:
```bash
azd env set APPLICATIONINSIGHTS_NAME "unique-name-$(date +%s)"
```

### Connection string not working
Verify:
1. Connection string is correctly copied
2. No extra spaces or quotes
3. Firewall allows outbound HTTPS to Azure

## Integration with CI/CD

### GitHub Actions

```yaml
name: Deploy Infrastructure

on:
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Install azd
        uses: Azure/setup-azd@v0.1.0
      
      - name: Azure Login
        run: |
          azd auth login --client-id ${{ secrets.AZURE_CLIENT_ID }} \
            --client-secret ${{ secrets.AZURE_CLIENT_SECRET }} \
            --tenant-id ${{ secrets.AZURE_TENANT_ID }}
      
      - name: Provision Infrastructure
        run: azd provision --no-prompt
        env:
          AZURE_ENV_NAME: ${{ secrets.AZURE_ENV_NAME }}
          AZURE_LOCATION: ${{ secrets.AZURE_LOCATION }}
          AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Azure DevOps

```yaml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - infra/AzureAppInsight/**

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: AzureCLI@2
  inputs:
    azureSubscription: 'Azure-Subscription'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      curl -fsSL https://aka.ms/install-azd.sh | bash
      azd provision --no-prompt
```

## Best Practices

1. **Use separate environments** for dev, staging, production
2. **Store connection strings securely** (Azure Key Vault, GitHub Secrets)
3. **Enable daily cap** to prevent unexpected costs
4. **Set up alerts** for high data ingestion
5. **Use workspace-based** Application Insights (not classic)
6. **Tag resources** appropriately for cost allocation

## Next Steps

1. **Deploy the infrastructure**: `azd provision`
2. **Update your app configuration** with the connection string
3. **Run your application** and generate telemetry
4. **View traces in Azure Portal** (may take 2-3 minutes)
5. **Set up alerts** for critical failures
6. **Create custom dashboards** for monitoring

## Additional Resources

- [Azure Developer CLI Documentation](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Application Insights Pricing](https://azure.microsoft.com/pricing/details/monitor/)
- [Application Insights Documentation](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)

## Support

For issues with:
- **Azure Developer CLI**: https://github.com/Azure/azure-dev
- **Bicep**: https://github.com/Azure/bicep
- **Application Insights**: Azure Portal → Support + Troubleshooting
