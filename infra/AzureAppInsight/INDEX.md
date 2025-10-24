# Azure Application Insights - Documentation Index

Welcome! This index helps you find the right documentation for your needs.

## ?? I Want To...

### Deploy to Azure (First Time)
**Start here:** `README.md` (this file)

Then follow:
1. Read the prerequisites section
2. Run the deployment script
3. Follow the checklist: `DEPLOYMENT_CHECKLIST.md`

### Deploy Quickly (Know What I'm Doing)
**Go to:** `QUICK_REFERENCE.md`

Quick commands:
```powershell
# Windows
.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings

# Linux/macOS
./infra/AzureAppInsight/deploy.sh -u
```

### Understand the Architecture
**Go to:** `ARCHITECTURE_OVERVIEW.md`

Learn about:
- Resource structure
- Deployment flow
- Cost breakdown
- Telemetry flow

### Troubleshoot Issues
**Go to:** `QUICK_REFERENCE.md` ? Troubleshooting section

Or check:
- Deployment issues ? `DEPLOYMENT_CHECKLIST.md`
- Application integration ? `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md`

### Integrate with My Application
**Go to:** `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md`

Learn about:
- Code configuration
- Viewing traces
- Writing KQL queries
- Setting up alerts

---

## ?? Complete Documentation Map

```
infra/AzureAppInsight/
??? ?? README.md
?   ??? Complete deployment guide
?       • Prerequisites
?       • Setup steps
?       • Manual deployment
?       • Best practices
?       • Resources
?
??? ?? QUICK_REFERENCE.md
?   ??? Quick command reference
?       • Common commands
?       • Azure regions
?       • Troubleshooting
?       • CI/CD examples
?
??? ?? ARCHITECTURE_OVERVIEW.md
?   ??? Visual architecture guide
?       • Architecture diagrams
?       • Deployment flow
?       • Cost breakdown
?       • Telemetry flow
?       • Comparison matrix
?
??? ? DEPLOYMENT_CHECKLIST.md
?   ??? Step-by-step checklist
?       • Pre-deployment checks
?       • Deployment steps
?       • Verification steps
?       • Testing procedures
?       • Production checklist
?
??? ?? COMPLETE.md
?   ??? Summary of everything
?       • Files created
?       • Quick start
?       • Features
?       • Success metrics
?
??? ?? deploy.ps1
?   ??? PowerShell deployment script
?
??? ?? deploy.sh
?   ??? Bash deployment script
?
??? ??? main.bicep
?   ??? Main Bicep template
?
??? ?? main.parameters.json
?   ??? Parameter configuration
?
??? ?? .env.template
?   ??? Environment variables template
?
??? ??? abbreviations.json
?   ??? Azure naming conventions
?
??? modules/
    ??? log-analytics.bicep
    ??? application-insights.bicep
```

---

## ?? Documentation by Role

### For Developers
**Primary documents:**
1. `README.md` - Understand the setup
2. `DEPLOYMENT_CHECKLIST.md` - Follow step-by-step
3. `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` - Integrate with code

**When you need:**
- Quick commands ? `QUICK_REFERENCE.md`
- Troubleshooting ? `QUICK_REFERENCE.md` or `DEPLOYMENT_CHECKLIST.md`

### For DevOps Engineers
**Primary documents:**
1. `ARCHITECTURE_OVERVIEW.md` - Understand infrastructure
2. `README.md` - Deployment details
3. `QUICK_REFERENCE.md` - CI/CD integration

**When you need:**
- Modify infrastructure ? `main.bicep` and `modules/*.bicep`
- Automate deployment ? `deploy.ps1` or `deploy.sh`
- Multiple environments ? `QUICK_REFERENCE.md` ? Environment section

### For Team Leads / Decision Makers
**Primary documents:**
1. `COMPLETE.md` - Executive summary
2. `ARCHITECTURE_OVERVIEW.md` - Cost analysis
3. `README.md` - Features and capabilities

**When you need:**
- Cost estimates ? `ARCHITECTURE_OVERVIEW.md` ? Cost Breakdown
- Success criteria ? `COMPLETE.md` ? Success Metrics
- ROI justification ? `COMPLETE.md` ? What You Now Have

---

## ?? Common Scenarios

### Scenario 1: First Time Deployment
```
1. Read: README.md (Prerequisites section)
2. Read: DEPLOYMENT_CHECKLIST.md
3. Run: deploy.ps1 or deploy.sh
4. Follow: DEPLOYMENT_CHECKLIST.md (step-by-step)
5. Verify: DEPLOYMENT_CHECKLIST.md (Verification section)
```

### Scenario 2: Quick Redeploy
```
1. Check: QUICK_REFERENCE.md
2. Run: azd provision
```

### Scenario 3: Production Deployment
```
1. Read: ARCHITECTURE_OVERVIEW.md (Security section)
2. Read: DEPLOYMENT_CHECKLIST.md (Production section)
3. Create: New environment (azd env new production)
4. Deploy: azd provision
5. Configure: Production connection string securely
```

### Scenario 4: Troubleshooting
```
1. Check: QUICK_REFERENCE.md (Troubleshooting section)
2. Check: DEPLOYMENT_CHECKLIST.md (Troubleshooting section)
3. Review: Error message carefully
4. Try: azd down && azd provision (clean redeploy)
```

### Scenario 5: Cost Optimization
```
1. Read: ARCHITECTURE_OVERVIEW.md (Cost Breakdown)
2. Monitor: Azure Portal ? Usage and estimated costs
3. Configure: Sampling in code
4. Set: Daily cap in Azure Portal
```

---

## ?? Quick Links by Topic

### Deployment
- **First time**: `README.md`
- **Quick deploy**: `QUICK_REFERENCE.md`
- **Checklist**: `DEPLOYMENT_CHECKLIST.md`
- **Automation**: `deploy.ps1` / `deploy.sh`

### Architecture
- **Overview**: `ARCHITECTURE_OVERVIEW.md`
- **Templates**: `main.bicep`, `modules/*.bicep`
- **Configuration**: `main.parameters.json`

### Integration
- **Application code**: `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md`
- **OpenTelemetry setup**: `../../Program.cs`
- **Configuration**: `../../appsettings.json`

### Operations
- **Commands**: `QUICK_REFERENCE.md`
- **Cost**: `ARCHITECTURE_OVERVIEW.md` ? Cost Breakdown
- **Security**: `ARCHITECTURE_OVERVIEW.md` ? Security section
- **CI/CD**: `QUICK_REFERENCE.md` ? CI/CD Integration

### Troubleshooting
- **Deployment**: `QUICK_REFERENCE.md` ? Troubleshooting
- **Application**: `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` ? Troubleshooting
- **Checklist**: `DEPLOYMENT_CHECKLIST.md` ? Troubleshooting

---

## ?? Search Guide

### I Need To Find...

**Prerequisites**
? `README.md` ? Prerequisites section

**Deployment Commands**
? `QUICK_REFERENCE.md` ? Common Commands

**Cost Information**
? `ARCHITECTURE_OVERVIEW.md` ? Cost Breakdown

**Connection String Setup**
? `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` ? Setup Steps

**Bicep Syntax**
? `main.bicep`, `modules/*.bicep`

**Environment Variables**
? `.env.template`

**Naming Conventions**
? `abbreviations.json`

**Error Solutions**
? `QUICK_REFERENCE.md` ? Troubleshooting

**Production Setup**
? `DEPLOYMENT_CHECKLIST.md` ? Production Deployment Checklist

**Security Best Practices**
? `ARCHITECTURE_OVERVIEW.md` ? Security & Best Practices

**KQL Queries**
? `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` ? Querying with KQL

**Cleanup Instructions**
? `DEPLOYMENT_CHECKLIST.md` ? Cleanup Checklist

---

## ?? Document Statistics

| Document | Purpose | Audience | Length |
|----------|---------|----------|--------|
| README.md | Complete guide | All | ~700 lines |
| QUICK_REFERENCE.md | Commands & tips | Dev/DevOps | ~400 lines |
| ARCHITECTURE_OVERVIEW.md | Architecture & cost | DevOps/Leads | ~650 lines |
| DEPLOYMENT_CHECKLIST.md | Step-by-step | Developers | ~500 lines |
| COMPLETE.md | Executive summary | All | ~450 lines |
| INDEX.md | Navigation | All | This file |

**Total documentation**: ~2,700+ lines across 6 comprehensive guides

---

## ?? Recommended Reading Order

### For First-Time Users
1. `COMPLETE.md` (10 min) - Get overview
2. `README.md` (20 min) - Understand setup
3. `DEPLOYMENT_CHECKLIST.md` (30 min) - Follow step-by-step
4. `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` (15 min) - Application integration

**Total**: ~75 minutes for complete understanding

### For Experienced Users
1. `QUICK_REFERENCE.md` (5 min) - Get commands
2. `ARCHITECTURE_OVERVIEW.md` (10 min) - Understand infrastructure
3. Deploy and learn by doing

**Total**: ~15 minutes to get started

---

## ?? Pro Tips

1. **Bookmark** `QUICK_REFERENCE.md` for daily use
2. **Print** `DEPLOYMENT_CHECKLIST.md` for first deployment
3. **Share** `COMPLETE.md` with team members
4. **Refer to** `ARCHITECTURE_OVERVIEW.md` for cost discussions
5. **Keep** `Help/AZURE_APPLICATION_INSIGHTS_INTEGRATION.md` open when coding

---

## ?? Still Can't Find What You Need?

### Check These Resources:

**Official Azure Documentation:**
- [Azure Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)

**Project Documentation:**
- Main README: `../../README.md`
- Help folder: `../../Help/`

**Support:**
- Azure Portal ? Support + Troubleshooting
- GitHub Issues (if this is a repo)

---

## ? Quick Start (TL;DR)

**Just want to deploy?**

1. Open: `DEPLOYMENT_CHECKLIST.md`
2. Run: `.\infra\AzureAppInsight\deploy.ps1 -UpdateAppSettings`
3. Wait: 5-10 minutes
4. Done: Connection string in appsettings.json

**Need help?** Start with `QUICK_REFERENCE.md` troubleshooting section.

---

**Happy deploying!** ??

*Last updated: When this infrastructure was created*
