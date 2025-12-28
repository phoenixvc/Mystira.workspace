#!/bin/bash
# Cosmos DB Migration Script
# Migrates data from legacy prodwusappmystiracosmos to new environment-specific accounts
#
# Source: prodwusappmystiracosmos (prod-wus-rg-mystira)
# Destinations:
#   - Production: mys-prod-core-cosmos-san (mys-prod-core-rg-san)
#   - Development: mys-dev-core-cosmos-san (mys-dev-core-rg-san)

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
CLI_PROJECT="$PROJECT_ROOT/Mystira.DevHub.CLI"
SOURCE_DATABASE="MystiraAppDb"
DEST_DATABASE="MystiraAppDb"

# Default values
DRY_RUN=false
ENVIRONMENT=""
MIGRATION_TYPE="all"
VERBOSE=false

# Usage function
usage() {
    echo -e "${BLUE}Cosmos DB Migration Script${NC}"
    echo ""
    echo "Migrates data from legacy Cosmos DB (prodwusappmystiracosmos) to new environment-specific accounts."
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -e, --environment ENV     Target environment: 'dev' or 'prod' (required)"
    echo "  -t, --type TYPE           Migration type: scenarios, bundles, media-metadata, user-profiles,"
    echo "                            game-sessions, accounts, compass-trackings, character-maps,"
    echo "                            badge-configurations, blobs, master-data, all (default: all)"
    echo "  -d, --dry-run             Preview mode - count items without migrating"
    echo "  -v, --verbose             Enable verbose output"
    echo "  -h, --help                Show this help message"
    echo ""
    echo "Environment Variables:"
    echo "  SOURCE_COSMOS_CONNECTION  Source Cosmos DB connection string (required)"
    echo "  DEST_COSMOS_CONNECTION    Destination Cosmos DB connection string (auto-detected from Azure if not set)"
    echo "  SOURCE_STORAGE_CONNECTION Source Azure Storage connection string (for blob migration)"
    echo "  DEST_STORAGE_CONNECTION   Destination Azure Storage connection string (for blob migration)"
    echo ""
    echo "Examples:"
    echo "  # Dry-run migration to dev environment"
    echo "  $0 --environment dev --dry-run"
    echo ""
    echo "  # Migrate all data to production"
    echo "  $0 --environment prod"
    echo ""
    echo "  # Migrate only scenarios to dev"
    echo "  $0 --environment dev --type scenarios"
    echo ""
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -t|--type)
            MIGRATION_TYPE="$2"
            shift 2
            ;;
        -d|--dry-run)
            DRY_RUN=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Validate required arguments
if [[ -z "$ENVIRONMENT" ]]; then
    log_error "Environment is required. Use --environment dev or --environment prod"
    usage
    exit 1
fi

if [[ "$ENVIRONMENT" != "dev" && "$ENVIRONMENT" != "prod" ]]; then
    log_error "Invalid environment: $ENVIRONMENT. Must be 'dev' or 'prod'"
    exit 1
fi

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi

    # Check if logged in to Azure
    if ! az account show &> /dev/null; then
        log_error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    fi

    # Check if .NET is installed
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK is not installed. Please install .NET 9.0 or later."
        exit 1
    fi

    log_success "Prerequisites check passed"
}

# Get connection strings from Azure
get_connection_strings() {
    log_info "Retrieving connection strings from Azure..."

    # Source Cosmos DB (legacy)
    if [[ -z "$SOURCE_COSMOS_CONNECTION" ]]; then
        log_info "Fetching source Cosmos DB connection string..."
        SOURCE_COSMOS_CONNECTION=$(az cosmosdb keys list \
            --name prodwusappmystiracosmos \
            --resource-group prod-wus-rg-mystira \
            --type connection-strings \
            --query "connectionStrings[0].connectionString" \
            -o tsv 2>/dev/null || echo "")

        if [[ -z "$SOURCE_COSMOS_CONNECTION" ]]; then
            log_error "Failed to retrieve source Cosmos DB connection string. Please set SOURCE_COSMOS_CONNECTION environment variable."
            exit 1
        fi
    fi

    # Destination Cosmos DB
    if [[ -z "$DEST_COSMOS_CONNECTION" ]]; then
        local dest_account=""
        local dest_rg=""

        if [[ "$ENVIRONMENT" == "prod" ]]; then
            dest_account="mys-prod-core-cosmos-san"
            dest_rg="mys-prod-core-rg-san"
        else
            dest_account="mys-dev-core-cosmos-san"
            dest_rg="mys-dev-core-rg-san"
        fi

        log_info "Fetching destination Cosmos DB connection string for $ENVIRONMENT..."
        DEST_COSMOS_CONNECTION=$(az cosmosdb keys list \
            --name "$dest_account" \
            --resource-group "$dest_rg" \
            --type connection-strings \
            --query "connectionStrings[0].connectionString" \
            -o tsv 2>/dev/null || echo "")

        if [[ -z "$DEST_COSMOS_CONNECTION" ]]; then
            log_error "Failed to retrieve destination Cosmos DB connection string for $ENVIRONMENT."
            log_error "Please set DEST_COSMOS_CONNECTION environment variable or ensure you have access to $dest_account in $dest_rg."
            exit 1
        fi
    fi

    log_success "Connection strings retrieved successfully"
}

# Build the CLI project
build_cli() {
    log_info "Building CLI project..."

    cd "$CLI_PROJECT"

    if [[ "$VERBOSE" == true ]]; then
        dotnet build --configuration Release
    else
        dotnet build --configuration Release --verbosity quiet
    fi

    if [[ $? -ne 0 ]]; then
        log_error "Failed to build CLI project"
        exit 1
    fi

    log_success "CLI project built successfully"
}

# Run the migration
run_migration() {
    log_info "Starting migration..."

    local dry_run_flag="false"
    if [[ "$DRY_RUN" == true ]]; then
        dry_run_flag="true"
        log_warning "Running in DRY-RUN mode - no data will be migrated"
    fi

    echo ""
    echo -e "${BLUE}Migration Configuration:${NC}"
    echo "  Environment: $ENVIRONMENT"
    echo "  Migration Type: $MIGRATION_TYPE"
    echo "  Source Database: $SOURCE_DATABASE"
    echo "  Destination Database: $DEST_DATABASE"
    echo "  Dry Run: $dry_run_flag"
    echo ""

    # Create the JSON command
    local json_command=$(cat <<EOF
{
    "command": "migration.run",
    "args": {
        "type": "$MIGRATION_TYPE",
        "sourceCosmosConnection": "$SOURCE_COSMOS_CONNECTION",
        "destCosmosConnection": "$DEST_COSMOS_CONNECTION",
        "sourceDatabaseName": "$SOURCE_DATABASE",
        "destDatabaseName": "$DEST_DATABASE",
        "dryRun": $dry_run_flag,
        "maxRetries": 3,
        "useBulkOperations": true
    }
}
EOF
)

    cd "$CLI_PROJECT"

    log_info "Executing migration command..."

    # Run the CLI and capture output
    # Use printf to avoid issues with echo on different platforms
    local result
    result=$(printf '%s' "$json_command" | dotnet run --configuration Release --no-build 2>&1)
    local exit_code=$?

    # Always show the result
    echo ""
    if [[ -n "$result" ]]; then
        echo -e "${GREEN}Migration Results:${NC}"
        # Try to pretty-print JSON, fall back to raw output
        echo "$result" | jq '.' 2>/dev/null || echo "$result"
        echo ""

        # Check if successful
        local success=$(echo "$result" | jq -r '.success // false' 2>/dev/null)
        if [[ "$success" == "true" ]]; then
            if [[ "$DRY_RUN" == true ]]; then
                log_success "Dry-run completed successfully!"
            else
                log_success "Migration completed successfully!"
            fi
        else
            log_warning "Migration completed with some issues. Check the results above."
        fi
    else
        log_warning "No output received from CLI. Exit code: $exit_code"
        if [[ $exit_code -ne 0 ]]; then
            log_error "Migration command failed with exit code $exit_code"
            exit 1
        fi
    fi
}

# Main execution
main() {
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}   Cosmos DB Migration Tool${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""

    check_prerequisites
    get_connection_strings
    build_cli
    run_migration

    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}   Migration Process Complete${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
}

main
