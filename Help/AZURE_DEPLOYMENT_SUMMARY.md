# Azure Deployment Summary

## What Was Created

A complete Azure infrastructure-as-code solution for deploying Application Insights using Azure Developer CLI (azd).

## File Structure

```
JaegerGettingStarted/
??? azure.yaml                                    # azd configuration
??? .azdignore                                    # azd ignore patterns
??? infra/
?   ??? .gitignore                                # Infrastructure gitignore
?   ??? AzureAppInsight/
?       ??? main.bicep                            # Main Bicep template
?       ??? main.parameters.json                  # Parameter configuration
?       ??? abbreviations.json                    # Azure naming conventions
?       ??? .env.template                         # Environment variables template
?       ??? README.md                             # Complete deployment guide
?       ??? QUICK_REFERENCE.md                    # Command quick reference
?       ??? ARCHITECTURE_OVERVIEW.md              # Visual architecture guide
?       ??? deploy.ps1                            # PowerShell deployment script
?       ??? deploy.sh                             # Bash deployment script
?       ??? modules/
?           ??? log-analytics.bicep               # Log Analytics module
?           ??? application-insights.bicep        # App Insights module
??? Help/
    ??? AZURE_APPLICATION_INSIGHTS_INTEGRATION.md # Integration guide
```

## Quick Start Options

### Option 1: Automated Script (Recommended)

**Windows PowerShell:**
```powershell
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

**Linux/macOS:**
```bash
./infra/AzureAppInsight/deploy.sh -u
```

### Option 2: Manual azd Commands

```bash
azd auth login
azd env new jaeger-demo
azd env set AZURE_ENV_NAME jaeger-demo
azd env set AZURE_LOCATION eastus
azd provision
```

## What the Scripts Do

1. ? Check prerequisites (azd, Azure CLI)
2. ? Authenticate to Azure
3. ? Create/select environment
4. ? Deploy infrastructure (Resource Group, Log Analytics, App Insights)
5. ? Retrieve connection string
6. ? Copy to clipboard (if available)
7. ? Update appsettings.json (if `-UpdateAppSettings` flag used)
8. ? Display next steps

## Resources Deployed

| Resource | Type | Purpose |
|----------|------|---------|
| Resource Group | `Microsoft.Resources/resourceGroups` | Container for all resources |
| Log Analytics Workspace | `Microsoft.OperationalInsights/workspaces` | Data storage and analytics |
| Application Insights | `Microsoft.Insights/components` | APM and monitoring |

## Configuration Applied

The application now sends telemetry to:
- ? **Jaeger** (localhost:4317) - Local development
- ? **Azure Application Insights** - Cloud monitoring

This is configured in `Program.cs`:
```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration
            .GetConnectionString("ApplicationInsights");
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            // ... existing Jaeger configuration ...
            .AddOtlpExporter(options => { ... }); // Jaeger
    });
```

## Cost Information

**Free Tier:**
- First 5 GB/month: $0
- 90-day retention: Included
- Basic queries: Included

**Paid Usage:**
- Additional data: ~$2.30/GB
- Extended retention: $0.10/GB/month

**Typical Development App:**
- Usually stays within free tier
- ~100-200 MB/day for light usage

## How to Use

### 1. Deploy Infrastructure
```bash
# Run deployment script
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

### 2. Run Your Application
```bash
dotnet run
```

### 3. Make Requests
```bash
curl https://localhost:7064/weatherforecast -k
curl https://localhost:7064/weather/London -k
```

### 4. View Traces

**Jaeger (Immediate):**
- http://localhost:16686
- Select "JaegerDemo.Api" service
- Click "Find Traces"

**Azure Portal (2-3 min delay):**
- Go to Azure Portal
- Navigate to your Application Insights resource
- Click "Transaction search" or "Application Map"

## Key Features

### Dual Monitoring
Send telemetry to both Jaeger and Azure simultaneously:
- **Jaeger**: Instant feedback during development
- **Azure**: Production-grade monitoring with alerts

### Infrastructure as Code
All resources defined in Bicep templates:
- Version controlled
- Repeatable deployments
- Multiple environments (dev, staging, prod)

### Automated Deployment
Scripts handle everything:
- Authentication
- Resource creation
- Configuration updates
- Connection string retrieval

### Production Ready
Built with best practices:
- Workspace-based App Insights (not classic)
- Proper resource naming conventions
- Secure connection string management
- Cost optimization (sampling, caps)

## Documentation Guide

| Document | When to Read |
|----------|-------------|
| `infra/AzureAppInsight/README.md` | First time deployment, detailed guide |
| `infra/AzureAppInsight/QUICK_REFERENCE.md` | Quick command lookup, troubleshooting |
| `infra/AzureAppInsight/ARCHITECTURE_OVERVIEW.md` | Understanding architecture, costs |
| `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` | Application integration, querying data |

## Common Commands

### Deploy Infrastructure
```bash
azd provision
```

### View Outputs
```bash
azd env get-values
```

### Update Infrastructure
```bash
# Modify Bicep files
azd provision  # Re-deploy
```

### Delete Resources
```bash
azd down
```

### Switch Environments
```bash
azd env new production
azd env select production
azd provision
```

## Troubleshooting

### No data in Azure?
1. Wait 2-3 minutes (ingestion delay)
2. Check connection string in appsettings.json
3. Verify firewall allows HTTPS to Azure
4. Check application logs for errors

### Deployment failed?
1. Check Azure subscription is active
2. Verify you have Contributor role
3. Try different Azure region
4. Check azd version: `azd version`

### Connection string not copying?
1. Manually retrieve: `azd env get-values`
2. Look for `APPLICATIONINSIGHTS_CONNECTION_STRING`
3. Copy and paste into appsettings.json

## Best Practices

### Development
- ? Use both Jaeger and Application Insights
- ? Keep connection string in User Secrets
- ? Stay within free tier (monitor usage)

### Production
- ? Use Application Insights only
- ? Store connection string in Key Vault
- ? Enable sampling for high volume
- ? Set daily data cap
- ? Configure alerts for critical failures

### Cost Management
- ? Monitor daily ingestion in Azure Portal
- ? Set up budget alerts
- ? Use sampling in production (50-90%)
- ? Configure daily cap (e.g., 1 GB)

## Security Checklist

- ? Connection string not committed to Git
- ? Use User Secrets for local development
- ? Use App Configuration for production
- ? Rotate instrumentation keys periodically
- ? Restrict access with RBAC
- ? Enable Azure AD authentication

## Next Steps

1. **Deploy infrastructure** using the script
2. **Test locally** with both Jaeger and Azure
3. **Explore Azure Portal** features
4. **Set up alerts** for critical errors
5. **Create custom dashboards** for your team
6. **Deploy to production** using same infrastructure

## Support

### Issues with Deployment
- Check `infra/AzureAppInsight/QUICK_REFERENCE.md` troubleshooting section
- Verify prerequisites are installed
- Check Azure subscription permissions

### Issues with Application Integration
- Check `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md`
- Verify connection string format
- Check OpenTelemetry configuration

### Azure Support
- Azure Portal ? Support + Troubleshooting
- Azure Developer CLI: https://github.com/Azure/azure-dev
- Bicep: https://github.com/Azure/bicep

## Summary

You now have:
- ? Complete infrastructure-as-code for Azure Application Insights
- ? Automated deployment scripts (PowerShell and Bash)
- ? Dual monitoring setup (Jaeger + Azure)
- ? Comprehensive documentation
- ? Production-ready architecture
- ? Cost-optimized configuration

**Total time to deploy**: ~5-10 minutes

**Cost for typical dev usage**: $0 (free tier)

**Ready to deploy?** Run `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
