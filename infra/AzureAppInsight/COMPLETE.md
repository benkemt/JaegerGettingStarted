# ?? Azure Application Insights Infrastructure - Complete!

## What Was Created

A **production-ready** Azure infrastructure-as-code solution for deploying Application Insights to your Azure subscription using **Azure Developer CLI (azd)**.

---

## ?? Files Created

### Infrastructure Files (12 files)

```
infra/
??? .gitignore                                    # Infrastructure gitignore
??? AzureAppInsight/
    ??? main.bicep                                # Main Bicep template ?
    ??? main.parameters.json                      # Parameter configuration
    ??? abbreviations.json                        # Azure naming conventions
    ??? .env.template                             # Environment variables template
    ??? README.md                                 # Complete deployment guide ??
    ??? QUICK_REFERENCE.md                        # Command quick reference ??
    ??? ARCHITECTURE_OVERVIEW.md                  # Visual architecture guide ??
    ??? DEPLOYMENT_CHECKLIST.md                   # Step-by-step checklist ?
    ??? deploy.ps1                                # PowerShell deployment script
    ??? deploy.sh                                 # Bash deployment script
    ??? modules/
        ??? log-analytics.bicep                   # Log Analytics module
        ??? application-insights.bicep            # Application Insights module
```

### Root Files (2 files)

```
JaegerGettingStarted/
??? azure.yaml                                    # azd project configuration
??? .azdignore                                    # azd ignore patterns
```

### Documentation Files (2 files)

```
Help/
??? AZURE_APPLICATION_INSIGHTS_INTEGRATION.md     # Integration guide
??? AZURE_DEPLOYMENT_SUMMARY.md                   # Quick summary
```

### Updated Files (2 files)

```
JaegerGettingStarted/
??? Program.cs                                    # Added Azure Monitor integration
??? appsettings.json                              # Added connection string placeholder
??? README.md                                     # Added Azure deployment section
```

**Total: 18 new/updated files** ??

---

## ?? Quick Start Guide

### Option 1: One-Command Deploy (Recommended)

**Windows PowerShell:**
```powershell
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

**Linux/macOS/WSL:**
```bash
chmod +x infra/AzureAppInsight/deploy.sh  # First time only
./infra/AzureAppInsight/deploy.sh -u
```

### Option 2: Manual Deployment

```bash
# Step 1: Login
azd auth login

# Step 2: Initialize environment
azd env new jaeger-demo
azd env set AZURE_ENV_NAME jaeger-demo
azd env set AZURE_LOCATION eastus

# Step 3: Deploy
azd provision

# Step 4: Get connection string
azd env get-values | grep APPLICATIONINSIGHTS_CONNECTION_STRING
```

---

## ?? What Gets Deployed

### Azure Resources

1. **Resource Group** (`rg-jaeger-demo`)
   - Container for all resources
   - Naming follows Azure best practices

2. **Log Analytics Workspace** (`log-xxxxxx`)
   - Data storage backend
   - 30-day retention (configurable)
   - PerGB2018 pricing tier

3. **Application Insights** (`appi-xxxxxx`)
   - APM and monitoring
   - 90-day retention (configurable)
   - Connected to Log Analytics
   - OpenTelemetry compatible

### Estimated Deployment Time
- **Infrastructure**: 2-5 minutes
- **First telemetry data**: +2-3 minutes
- **Total**: ~5-10 minutes

---

## ?? Cost Information

### Free Tier (Included)
- ? First 5 GB/month data ingestion
- ? 90-day data retention
- ? Unlimited queries
- ? Application Map
- ? Transaction Search
- ? Live Metrics

### Typical Usage
| Scenario | Daily Data | Monthly Data | Estimated Cost |
|----------|-----------|--------------|----------------|
| **Small Dev App** | 100 MB | ~3 GB | **$0** (Free tier) |
| **Medium App** | 500 MB | ~15 GB | **~$23** |
| **Large App** | 2 GB | ~60 GB | **~$126** |

### Cost Control
- Set daily data cap (e.g., 1 GB/day)
- Enable sampling for production (50-90%)
- Monitor usage in Azure Portal

---

## ? Features Implemented

### Infrastructure as Code
- ? Bicep templates (ARM alternative)
- ? Modular architecture (reusable modules)
- ? Parameter-driven configuration
- ? Azure naming conventions
- ? Version controlled

### Automation
- ? PowerShell deployment script (Windows)
- ? Bash deployment script (Linux/macOS)
- ? Automatic authentication flow
- ? Environment management
- ? Connection string retrieval
- ? Configuration file updates

### Application Integration
- ? Dual monitoring (Jaeger + Azure)
- ? OpenTelemetry SDK configured
- ? Azure Monitor exporter
- ? Automatic exception tracking
- ? Database query tracing (EF Core)
- ? HTTP request tracing

### Documentation
- ? Complete deployment guide
- ? Quick reference guide
- ? Architecture overview
- ? Deployment checklist
- ? Integration guide
- ? Troubleshooting guide

---

## ?? Documentation Guide

| When You Need | Read This Document |
|---------------|-------------------|
| **First-time setup** | `infra/AzureAppInsight/README.md` |
| **Quick commands** | `infra/AzureAppInsight/QUICK_REFERENCE.md` |
| **Architecture info** | `infra/AzureAppInsight/ARCHITECTURE_OVERVIEW.md` |
| **Step-by-step deploy** | `infra/AzureAppInsight/DEPLOYMENT_CHECKLIST.md` |
| **App integration** | `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` |
| **Quick overview** | `Help/AZURE_DEPLOYMENT_SUMMARY.md` |

---

## ?? How It Works

### Architecture Flow

```
Developer Workstation
    ?
    ? 1. Run deploy script
    ?
Azure Developer CLI (azd)
    ?
    ? 2. Execute Bicep templates
    ?
Azure Resource Manager
    ?
    ? 3. Create resources
    ?
Azure Resources Created:
  • Resource Group
  • Log Analytics Workspace
  • Application Insights
    ?
    ? 4. Return connection string
    ?
Configuration Updated:
  • appsettings.json (optional)
  • Clipboard (if available)
    ?
    ? 5. Application sends telemetry
    ?
Dual Monitoring:
  • Jaeger (localhost) ? Instant feedback
  • Azure App Insights ? Production monitoring
```

### Telemetry Flow

```
.NET Application
    ?
    ???? OpenTelemetry SDK
    ?    ???? ASP.NET Core instrumentation
    ?    ???? HttpClient instrumentation
    ?    ???? EF Core instrumentation
    ?    ???? Custom spans
    ?
    ???? Azure Monitor Exporter ? Azure Application Insights
    ?                              ???? Transaction Search
    ?                              ???? Application Map
    ?                              ???? Performance View
    ?                              ???? Failures View
    ?                              ???? Live Metrics
    ?
    ???? OTLP Exporter ? Jaeger (localhost:4317)
                          ???? Jaeger UI (localhost:16686)
```

---

## ?? Security Best Practices

### ? What We Did Right
- Connection string placeholder (not real key)
- `.gitignore` for sensitive files
- Environment variable support
- User Secrets recommended

### ? What NOT to Do
- Don't commit real connection strings to Git
- Don't hardcode secrets in code
- Don't share connection strings in chat/email

### ? Recommended Approach
**Development:**
```bash
dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "your-key"
```

**Production:**
- Azure App Service: Application Settings
- Azure Key Vault: Secrets
- Azure App Configuration: Configuration
- Environment Variables: Container Apps/Kubernetes

---

## ?? Testing the Deployment

### Step 1: Deploy Infrastructure
```powershell
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings
```

### Step 2: Run Application
```bash
dotnet run
```

### Step 3: Make Test Requests
```bash
# Weather forecast
curl https://localhost:7064/weatherforecast -k

# Save to database
curl -X POST https://localhost:7064/weather/record -k \
  -H "Content-Type: application/json" \
  -d '{"city":"London","temperature":18,"summary":"Mild"}'

# Trigger exception
curl https://localhost:7064/weather/Tokyo -k
```

### Step 4: View in Jaeger (Instant)
- Open: http://localhost:16686
- Service: "JaegerDemo.Api"
- Click "Find Traces"

### Step 5: View in Azure (2-3 min delay)
- Open: https://portal.azure.com
- Navigate to Application Insights
- Click "Transaction search"
- Set time range: Last 30 minutes

---

## ?? What You Learned

### Infrastructure as Code (IaC)
- ? Bicep template syntax
- ? Resource dependencies
- ? Parameter management
- ? Module composition
- ? Output variables

### Azure Developer CLI (azd)
- ? Environment management
- ? Infrastructure provisioning
- ? Configuration handling
- ? Resource lifecycle

### Azure Application Insights
- ? Workspace-based setup
- ? OpenTelemetry integration
- ? Transaction search
- ? Application Map
- ? KQL queries

### DevOps Best Practices
- ? Automation scripts
- ? Repeatable deployments
- ? Multiple environments
- ? Cost management
- ? Security practices

---

## ?? Next Steps

### Immediate (First Day)
1. ? Deploy infrastructure
2. ? Test locally with Jaeger
3. ? Verify data in Azure Portal
4. ? Explore Transaction Search

### Short-term (First Week)
1. Set up alerts for failures
2. Create custom dashboard
3. Learn KQL queries
4. Configure sampling

### Long-term (Production)
1. Deploy production environment
2. Integrate with CI/CD
3. Set up budget alerts
4. Train team members

---

## ??? Troubleshooting

### Common Issues

**No data in Azure?**
- Wait 2-3 minutes (ingestion delay)
- Check connection string
- Verify network connectivity

**Deployment failed?**
- Check Azure subscription
- Verify permissions (Contributor role)
- Try different region

**Build errors?**
- Ensure .NET 9 SDK installed
- Restore packages: `dotnet restore`
- Check package references

### Get Help
- **Quick fixes**: `infra/AzureAppInsight/QUICK_REFERENCE.md`
- **Detailed guide**: `infra/AzureAppInsight/README.md`
- **Azure support**: Azure Portal ? Support
- **azd issues**: https://github.com/Azure/azure-dev

---

## ?? Cleanup

### Delete Azure Resources
```bash
# Delete everything
azd down

# Delete with confirmation
azd down --force

# Delete and purge
azd down --purge
```

### Local Cleanup
```bash
# Stop application
Ctrl+C

# Stop Jaeger
docker-compose down

# Clear user secrets
dotnet user-secrets clear
```

---

## ?? Success Metrics

### You're Ready When:
- ? Infrastructure deploys without errors
- ? Application runs successfully
- ? Traces appear in both Jaeger and Azure
- ? Exceptions are captured
- ? Database queries are traced
- ? Application Map shows dependencies

---

## ?? Summary

### What You Now Have:
1. **Production-ready infrastructure** for Azure Application Insights
2. **Automated deployment scripts** (PowerShell + Bash)
3. **Comprehensive documentation** (6 detailed guides)
4. **Dual monitoring setup** (Jaeger + Azure)
5. **Best practices implemented** (IaC, security, cost optimization)
6. **Ready for production** (scalable, secure, observable)

### Time Investment:
- **Setup**: 5-10 minutes
- **Learning**: 1-2 hours (reading docs)
- **Value**: Priceless (production observability!)

### Cost:
- **Development**: $0 (free tier)
- **Small production**: ~$23/month (15 GB)
- **Enterprise**: Scales with usage

---

## ?? Key Takeaways

1. **OpenTelemetry is vendor-agnostic** - Send to Jaeger, Azure, or both
2. **Infrastructure as Code is powerful** - Repeatable, version-controlled deployments
3. **Azure Developer CLI simplifies deployment** - One command to deploy everything
4. **Dual monitoring is best for learning** - Jaeger for dev, Azure for prod
5. **Documentation matters** - 6 guides ensure success

---

## ?? Ready to Deploy?

```powershell
# Windows
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings

# Linux/macOS
./infra/AzureAppInsight/deploy.sh -u
```

**Deployment time**: 5-10 minutes
**Cost**: $0 for typical dev usage
**Value**: Production-grade observability! ??

---

**Questions?** Check the documentation in `infra/AzureAppInsight/` and `Help/` folders!

**Happy Monitoring!** ??????
