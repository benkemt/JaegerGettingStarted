#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Deploy Azure Application Insights infrastructure using Azure Developer CLI (azd).

.DESCRIPTION
    This script simplifies the deployment of Application Insights by:
    - Checking prerequisites (azd, Azure CLI)
    - Setting up environment variables
    - Running azd provision
    - Updating appsettings.json with the connection string

.PARAMETER EnvironmentName
    The name of the environment (e.g., dev, staging, prod)

.PARAMETER Location
    The Azure region for deployment (e.g., eastus, westus2)

.PARAMETER SubscriptionId
    (Optional) Azure subscription ID

.PARAMETER UpdateAppSettings
    (Optional) Automatically update appsettings.json with connection string

.EXAMPLE
    .\deploy.ps1 -EnvironmentName "jaeger-demo-dev" -Location "eastus"

.EXAMPLE
    .\deploy.ps1 -EnvironmentName "prod" -Location "westus2" -UpdateAppSettings

#>

param(
    [Parameter(Mandatory = $false)]
    [string]$EnvironmentName = "jaeger-demo",

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $false)]
    [string]$SubscriptionId = "",

    [Parameter(Mandatory = $false)]
    [switch]$UpdateAppSettings
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Azure Application Insights Deployment Script" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command {
    param($Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

if (-not (Test-Command "azd")) {
    Write-Host "? Azure Developer CLI (azd) is not installed." -ForegroundColor Red
    Write-Host "   Install it with: winget install microsoft.azd" -ForegroundColor Yellow
    Write-Host "   Or visit: https://aka.ms/install-azd" -ForegroundColor Yellow
    exit 1
}
Write-Host "? Azure Developer CLI (azd) found" -ForegroundColor Green

if (-not (Test-Command "az")) {
    Write-Host "? Azure CLI (az) is not installed. It's recommended but not required." -ForegroundColor Yellow
    Write-Host "   Install it with: winget install -e --id Microsoft.AzureCLI" -ForegroundColor Yellow
}
else {
    Write-Host "? Azure CLI (az) found" -ForegroundColor Green
}

Write-Host ""

# Initialize azd environment
Write-Host "Setting up environment..." -ForegroundColor Yellow
Write-Host "Environment Name: $EnvironmentName" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan

# Check if environment already exists
$envExists = $false
try {
    $currentEnv = azd env list 2>&1 | Select-String -Pattern $EnvironmentName
    if ($currentEnv) {
        $envExists = $true
        Write-Host "? Environment '$EnvironmentName' already exists" -ForegroundColor Green
    }
}
catch {
    # Environment doesn't exist yet
}

if (-not $envExists) {
    Write-Host "Creating new environment '$EnvironmentName'..." -ForegroundColor Yellow
    azd env new $EnvironmentName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to create environment" -ForegroundColor Red
        exit 1
    }
}

# Set environment variables
Write-Host "Setting environment variables..." -ForegroundColor Yellow
azd env set AZURE_ENV_NAME $EnvironmentName
azd env set AZURE_LOCATION $Location

if ($SubscriptionId) {
    azd env set AZURE_SUBSCRIPTION_ID $SubscriptionId
}

Write-Host ""

# Check if user is logged in
Write-Host "Checking Azure authentication..." -ForegroundColor Yellow
$loginCheck = azd auth login --check-status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not logged in to Azure. Starting login process..." -ForegroundColor Yellow
    azd auth login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to login to Azure" -ForegroundColor Red
        exit 1
    }
}
Write-Host "? Authenticated to Azure" -ForegroundColor Green
Write-Host ""

# Provision infrastructure
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Provisioning Azure Infrastructure..." -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This may take 2-5 minutes..." -ForegroundColor Yellow
Write-Host ""

azd provision

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Provisioning failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Green
Write-Host "? Infrastructure provisioned successfully!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""

# Get outputs
Write-Host "Retrieving deployment outputs..." -ForegroundColor Yellow
$envVars = azd env get-values

# Parse connection string
$connectionString = ""
foreach ($line in $envVars) {
    if ($line -match "APPLICATIONINSIGHTS_CONNECTION_STRING=(.+)") {
        $connectionString = $Matches[1].Trim('"')
        break
    }
}

if ($connectionString) {
    Write-Host "? Application Insights Connection String retrieved" -ForegroundColor Green
    Write-Host ""
    Write-Host "Connection String:" -ForegroundColor Cyan
    Write-Host $connectionString -ForegroundColor White
    Write-Host ""
    
    # Copy to clipboard if available
    if (Test-Command "Set-Clipboard") {
        $connectionString | Set-Clipboard
        Write-Host "? Connection string copied to clipboard!" -ForegroundColor Green
        Write-Host ""
    }

    # Update appsettings.json if requested
    if ($UpdateAppSettings) {
        Write-Host "Updating appsettings.json..." -ForegroundColor Yellow
        
        $appSettingsPath = "appsettings.json"
        if (Test-Path $appSettingsPath) {
            $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
            
            # Ensure ConnectionStrings object exists
            if (-not $appSettings.ConnectionStrings) {
                $appSettings | Add-Member -Type NoteProperty -Name ConnectionStrings -Value ([PSCustomObject]@{})
            }
            
            # Update or add ApplicationInsights connection string
            if ($appSettings.ConnectionStrings.PSObject.Properties['ApplicationInsights']) {
                $appSettings.ConnectionStrings.ApplicationInsights = $connectionString
            }
            else {
                $appSettings.ConnectionStrings | Add-Member -Type NoteProperty -Name ApplicationInsights -Value $connectionString
            }
            
            # Save back to file
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
            
            Write-Host "? appsettings.json updated successfully!" -ForegroundColor Green
            Write-Host ""
        }
        else {
            Write-Host "? appsettings.json not found in current directory" -ForegroundColor Yellow
            Write-Host "   Please update it manually with the connection string above" -ForegroundColor Yellow
            Write-Host ""
        }
    }
}
else {
    Write-Host "? Could not retrieve connection string" -ForegroundColor Yellow
    Write-Host "   Run 'azd env get-values' to see all outputs" -ForegroundColor Yellow
}

# Display resource group info
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. View your resources in Azure Portal:" -ForegroundColor White
Write-Host "   https://portal.azure.com/#view/HubsExtension/BrowseResourceGroups" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Update your application configuration:" -ForegroundColor White
if (-not $UpdateAppSettings) {
    Write-Host "   Add the connection string to appsettings.json" -ForegroundColor Cyan
    Write-Host "   Or run this script with -UpdateAppSettings flag" -ForegroundColor Cyan
}
else {
    Write-Host "   ? Already updated!" -ForegroundColor Green
}
Write-Host ""
Write-Host "3. Run your application:" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. View telemetry in Application Insights:" -ForegroundColor White
Write-Host "   (Wait 2-3 minutes for data to appear)" -ForegroundColor Yellow
Write-Host ""
Write-Host "5. To delete resources when done:" -ForegroundColor White
Write-Host "   azd down" -ForegroundColor Cyan
Write-Host ""
Write-Host "==================================================" -ForegroundColor Green
Write-Host "Deployment Complete! ??" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
