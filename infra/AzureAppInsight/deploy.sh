#!/bin/bash

###############################################################################
# Deploy Azure Application Insights infrastructure using Azure Developer CLI
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT_NAME="jaeger-demo"
LOCATION="eastus"
SUBSCRIPTION_ID=""
UPDATE_APP_SETTINGS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT_NAME="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -s|--subscription)
            SUBSCRIPTION_ID="$2"
            shift 2
            ;;
        -u|--update-appsettings)
            UPDATE_APP_SETTINGS=true
            shift
            ;;
        -h|--help)
            echo "Usage: ./deploy.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -e, --environment NAME       Environment name (default: jaeger-demo)"
            echo "  -l, --location REGION        Azure region (default: eastus)"
            echo "  -s, --subscription ID        Azure subscription ID (optional)"
            echo "  -u, --update-appsettings     Update appsettings.json with connection string"
            echo "  -h, --help                   Show this help message"
            echo ""
            echo "Example:"
            echo "  ./deploy.sh -e prod -l westus2 -u"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}Azure Application Insights Deployment Script${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v azd &> /dev/null; then
    echo -e "${RED}? Azure Developer CLI (azd) is not installed.${NC}"
    echo -e "${YELLOW}   Install it with: curl -fsSL https://aka.ms/install-azd.sh | bash${NC}"
    exit 1
fi
echo -e "${GREEN}? Azure Developer CLI (azd) found${NC}"

if ! command -v az &> /dev/null; then
    echo -e "${YELLOW}? Azure CLI (az) is not installed. It's recommended but not required.${NC}"
else
    echo -e "${GREEN}? Azure CLI (az) found${NC}"
fi

if command -v jq &> /dev/null; then
    echo -e "${GREEN}? jq found (for JSON processing)${NC}"
else
    echo -e "${YELLOW}? jq not found. JSON updates may not work. Install with: sudo apt install jq${NC}"
fi

echo ""

# Setup environment
echo -e "${YELLOW}Setting up environment...${NC}"
echo -e "${CYAN}Environment Name: $ENVIRONMENT_NAME${NC}"
echo -e "${CYAN}Location: $LOCATION${NC}"

# Check if environment exists
if azd env list 2>&1 | grep -q "$ENVIRONMENT_NAME"; then
    echo -e "${GREEN}? Environment '$ENVIRONMENT_NAME' already exists${NC}"
else
    echo -e "${YELLOW}Creating new environment '$ENVIRONMENT_NAME'...${NC}"
    azd env new "$ENVIRONMENT_NAME"
fi

# Set environment variables
echo -e "${YELLOW}Setting environment variables...${NC}"
azd env set AZURE_ENV_NAME "$ENVIRONMENT_NAME"
azd env set AZURE_LOCATION "$LOCATION"

if [ -n "$SUBSCRIPTION_ID" ]; then
    azd env set AZURE_SUBSCRIPTION_ID "$SUBSCRIPTION_ID"
fi

echo ""

# Check authentication
echo -e "${YELLOW}Checking Azure authentication...${NC}"
if ! azd auth login --check-status &> /dev/null; then
    echo -e "${YELLOW}Not logged in to Azure. Starting login process...${NC}"
    azd auth login
fi
echo -e "${GREEN}? Authenticated to Azure${NC}"
echo ""

# Provision infrastructure
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}Provisioning Azure Infrastructure...${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""
echo -e "${YELLOW}This may take 2-5 minutes...${NC}"
echo ""

azd provision

echo ""
echo -e "${GREEN}==================================================${NC}"
echo -e "${GREEN}? Infrastructure provisioned successfully!${NC}"
echo -e "${GREEN}==================================================${NC}"
echo ""

# Get outputs
echo -e "${YELLOW}Retrieving deployment outputs...${NC}"
CONNECTION_STRING=$(azd env get-values | grep APPLICATIONINSIGHTS_CONNECTION_STRING | cut -d'=' -f2- | tr -d '"')

if [ -n "$CONNECTION_STRING" ]; then
    echo -e "${GREEN}? Application Insights Connection String retrieved${NC}"
    echo ""
    echo -e "${CYAN}Connection String:${NC}"
    echo -e "${NC}$CONNECTION_STRING${NC}"
    echo ""
    
    # Copy to clipboard if available
    if command -v pbcopy &> /dev/null; then
        echo "$CONNECTION_STRING" | pbcopy
        echo -e "${GREEN}? Connection string copied to clipboard! (macOS)${NC}"
        echo ""
    elif command -v xclip &> /dev/null; then
        echo "$CONNECTION_STRING" | xclip -selection clipboard
        echo -e "${GREEN}? Connection string copied to clipboard! (Linux)${NC}"
        echo ""
    elif command -v wl-copy &> /dev/null; then
        echo "$CONNECTION_STRING" | wl-copy
        echo -e "${GREEN}? Connection string copied to clipboard! (Wayland)${NC}"
        echo ""
    fi

    # Update appsettings.json if requested
    if [ "$UPDATE_APP_SETTINGS" = true ]; then
        echo -e "${YELLOW}Updating appsettings.json...${NC}"
        
        APP_SETTINGS_PATH="appsettings.json"
        if [ -f "$APP_SETTINGS_PATH" ]; then
            if command -v jq &> /dev/null; then
                # Use jq to update the JSON
                jq --arg conn "$CONNECTION_STRING" \
                   '.ConnectionStrings.ApplicationInsights = $conn' \
                   "$APP_SETTINGS_PATH" > "$APP_SETTINGS_PATH.tmp" \
                   && mv "$APP_SETTINGS_PATH.tmp" "$APP_SETTINGS_PATH"
                
                echo -e "${GREEN}? appsettings.json updated successfully!${NC}"
                echo ""
            else
                echo -e "${YELLOW}? jq not installed. Cannot update appsettings.json automatically${NC}"
                echo -e "${YELLOW}   Please update it manually with the connection string above${NC}"
                echo ""
            fi
        else
            echo -e "${YELLOW}? appsettings.json not found in current directory${NC}"
            echo -e "${YELLOW}   Please update it manually with the connection string above${NC}"
            echo ""
        fi
    fi
else
    echo -e "${YELLOW}? Could not retrieve connection string${NC}"
    echo -e "${YELLOW}   Run 'azd env get-values' to see all outputs${NC}"
fi

# Display next steps
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}Next Steps${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""
echo -e "${NC}1. View your resources in Azure Portal:${NC}"
echo -e "${CYAN}   https://portal.azure.com/#view/HubsExtension/BrowseResourceGroups${NC}"
echo ""
echo -e "${NC}2. Update your application configuration:${NC}"
if [ "$UPDATE_APP_SETTINGS" = false ]; then
    echo -e "${CYAN}   Add the connection string to appsettings.json${NC}"
    echo -e "${CYAN}   Or run this script with --update-appsettings flag${NC}"
else
    echo -e "${GREEN}   ? Already updated!${NC}"
fi
echo ""
echo -e "${NC}3. Run your application:${NC}"
echo -e "${CYAN}   dotnet run${NC}"
echo ""
echo -e "${NC}4. View telemetry in Application Insights:${NC}"
echo -e "${YELLOW}   (Wait 2-3 minutes for data to appear)${NC}"
echo ""
echo -e "${NC}5. To delete resources when done:${NC}"
echo -e "${CYAN}   azd down${NC}"
echo ""
echo -e "${GREEN}==================================================${NC}"
echo -e "${GREEN}Deployment Complete! ??${NC}"
echo -e "${GREEN}==================================================${NC}"
