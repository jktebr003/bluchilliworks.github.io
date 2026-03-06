# BluChilli Works - Orchestration Helper Script
# Quick commands to manage both Aspire and Docker Compose orchestration

param(
    [Parameter(Position=0)]
    [ValidateSet('aspire', 'docker-dev', 'docker-prod', 'docker-stop', 'docker-clean', 'docker-push', 'docker-push-api', 'docker-push-web', 'status', 'help')]
    [string]$Command = 'help',
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$RootPath = Split-Path -Parent $MyInvocation.MyCommand.Path

function Show-Help {
    Write-Host ""
    Write-Host "BluChilli Works - Orchestration Helper" -ForegroundColor Cyan
    Write-Host "======================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\orchestrate.ps1 [command]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Green
    Write-Host "  aspire           - Start with .NET Aspire (development)" -ForegroundColor White
    Write-Host "  docker-dev       - Start with Docker Compose (development mode)" -ForegroundColor White
    Write-Host "  docker-prod      - Start with Docker Compose (production mode)" -ForegroundColor White
    Write-Host "  docker-stop      - Stop Docker Compose services" -ForegroundColor White
    Write-Host "  docker-clean     - Stop services and remove volumes (clean slate)" -ForegroundColor White
    Write-Host "  docker-push      - Build and push both API and Web to Docker Hub" -ForegroundColor White
    Write-Host "  docker-push-api  - Build and push API image only to Docker Hub" -ForegroundColor White
    Write-Host "  docker-push-web  - Build and push Web image only to Docker Hub" -ForegroundColor White
    Write-Host "  status           - Show status of Docker services" -ForegroundColor White
    Write-Host "  help             - Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Docker Push Options:" -ForegroundColor Green
    Write-Host "  -Username <name>     - Docker Hub username (required)" -ForegroundColor White
    Write-Host "  -Version <version>   - Image version tag (default: latest)" -ForegroundColor White
    Write-Host "  -SkipBuild           - Push existing images without rebuilding" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Green
    Write-Host "  .\orchestrate.ps1 aspire" -ForegroundColor Gray
    Write-Host "  .\orchestrate.ps1 docker-dev" -ForegroundColor Gray
    Write-Host "  .\orchestrate.ps1 docker-push -Username yourusername -Version 1.0.0" -ForegroundColor Gray
    Write-Host "  .\orchestrate.ps1 docker-push-api -Username yourusername" -ForegroundColor Gray
    Write-Host ""
}

function Start-Aspire {
    Write-Host ""
    Write-Host "Starting .NET Aspire..." -ForegroundColor Cyan
    Write-Host "Dashboard will open automatically" -ForegroundColor Yellow
    Write-Host ""
    Set-Location "$RootPath\AppHost"
    dotnet restore
    dotnet run
}

function Start-DockerDev {
    Write-Host ""
    Write-Host "Starting Docker Compose (Development Mode)..." -ForegroundColor Cyan
    Write-Host "Hot reload enabled" -ForegroundColor Yellow
    Write-Host ""
    
    # Prefer .env.development for development mode
    if (-not (Test-Path "$RootPath\.env")) {
        if (Test-Path "$RootPath\.env.development") {
            Write-Host ".env file not found. Creating from .env.development..." -ForegroundColor Yellow
            Copy-Item "$RootPath\.env.development" "$RootPath\.env"
        } elseif (Test-Path "$RootPath\.env.example") {
            Write-Host ".env file not found. Creating from .env.example..." -ForegroundColor Yellow
            Copy-Item "$RootPath\.env.example" "$RootPath\.env"
        } else {
            Write-Host ".env file not found and no .env.development or .env.example available!" -ForegroundColor Red
            exit 1
        }
    }
    
    Set-Location $RootPath
    docker-compose -f docker-compose.windows.yml up --build
}

function Start-DockerProd {
    Write-Host ""
    Write-Host "Starting Docker Compose (Production Mode)..." -ForegroundColor Cyan
    Write-Host "Optimized builds, no hot reload" -ForegroundColor Yellow
    Write-Host ""
    
    # Check if .env exists
    if (-not (Test-Path "$RootPath\.env")) {
        Write-Host ".env file required for production!" -ForegroundColor Red
        Write-Host "   Copy .env.production.template to .env and configure" -ForegroundColor Yellow
        exit 1
    }
    
    Set-Location $RootPath
    docker-compose -f docker-compose.windows.yml up --build -d
    
    Write-Host ""
    Write-Host "Services started in detached mode" -ForegroundColor Green
    Write-Host "   View logs: docker-compose logs -f" -ForegroundColor Gray
    Write-Host "   Check status: .\orchestrate.ps1 status" -ForegroundColor Gray
}

function Stop-Docker {
    Write-Host ""
    Write-Host "Stopping Docker Compose services..." -ForegroundColor Cyan
    Set-Location $RootPath
    docker-compose down
    Write-Host "Services stopped" -ForegroundColor Green
}

function Clean-Docker {
    Write-Host ""
    Write-Host "Stopping services and removing volumes..." -ForegroundColor Cyan
    Write-Host "This will delete all MongoDB data!" -ForegroundColor Yellow
    
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -eq "yes") {
        Set-Location $RootPath
        docker-compose down -v
        Write-Host "Services stopped and volumes removed" -ForegroundColor Green
    } else {
        Write-Host "Cancelled" -ForegroundColor Yellow
    }
}

function Push-DockerImages {
    param(
        [string]$DockerUsername,
        [string]$ImageVersion,
        [bool]$SkipImageBuild,
        [bool]$ApiOnly = $false,
        [bool]$WebOnly = $false
    )
    
    Write-Host ""
    Write-Host "Docker Hub Push" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    Write-Host ""
    
    # Prompt for username if not provided
    if ([string]::IsNullOrWhiteSpace($DockerUsername)) {
        $DockerUsername = Read-Host "Enter your Docker Hub username"
        
        if ([string]::IsNullOrWhiteSpace($DockerUsername)) {
            Write-Host "Error: Docker Hub username is required" -ForegroundColor Red
            exit 1
        }
    }
    
    # Prompt for version if not provided, default to 'latest'
    if ([string]::IsNullOrWhiteSpace($ImageVersion)) {
        $versionInput = Read-Host "Enter version tag (press Enter for 'latest')"
        
        if ([string]::IsNullOrWhiteSpace($versionInput)) {
            $ImageVersion = "latest"
        } else {
            $ImageVersion = $versionInput
        }
    }
    
    Write-Host ""
    Write-Host "Configuration:" -ForegroundColor Green
    Write-Host "  Username: $DockerUsername" -ForegroundColor White
    Write-Host "  Version: $ImageVersion" -ForegroundColor White
    Write-Host "  Skip Build: $SkipImageBuild" -ForegroundColor White
    Write-Host ""
    
    Set-Location $RootPath
    
    # Check if build-and-push.ps1 exists
    if (-not (Test-Path ".\build-and-push.ps1")) {
        Write-Host "Error: build-and-push.ps1 not found" -ForegroundColor Red
        exit 1
    }
    
    # Build arguments hashtable for splatting
    $pushArgs = @{
        Username = $DockerUsername
        Version = $ImageVersion
    }
    
    if ($SkipImageBuild) {
        $pushArgs['SkipBuild'] = $true
    }
    
    if ($ApiOnly) {
        $pushArgs['ApiOnly'] = $true
        Write-Host "Pushing API image only..." -ForegroundColor Yellow
    }
    elseif ($WebOnly) {
        $pushArgs['WebOnly'] = $true
        Write-Host "Pushing Web image only..." -ForegroundColor Yellow
    }
    else {
        Write-Host "Pushing both API and Web images..." -ForegroundColor Yellow
    }
    
    Write-Host ""
    & ".\build-and-push.ps1" @pushArgs
}

function Show-Status {
    Write-Host ""
    Write-Host "Docker Services Status" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    Write-Host ""
    
    Set-Location $RootPath
    docker-compose ps
    
    Write-Host ""
    Write-Host "Health Checks:" -ForegroundColor Green
    Write-Host "  API:  http://localhost:5000/health" -ForegroundColor Gray
    Write-Host "  Web:  http://localhost:5001/" -ForegroundColor Gray
    Write-Host ""
}

# Main execution
switch ($Command) {
    'aspire'          { Start-Aspire }
    'docker-dev'      { Start-DockerDev }
    'docker-prod'     { Start-DockerProd }
    'docker-stop'     { Stop-Docker }
    'docker-clean'    { Clean-Docker }
    'docker-push'     { Push-DockerImages -DockerUsername $Username -ImageVersion $Version -SkipImageBuild $SkipBuild.IsPresent }
    'docker-push-api' { Push-DockerImages -DockerUsername $Username -ImageVersion $Version -SkipImageBuild $SkipBuild.IsPresent -ApiOnly $true }
    'docker-push-web' { Push-DockerImages -DockerUsername $Username -ImageVersion $Version -SkipImageBuild $SkipBuild.IsPresent -WebOnly $true }
    'status'          { Show-Status }
    'help'            { Show-Help }
    default           { Show-Help }
}
