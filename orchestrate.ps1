# BluChilli Works - Orchestration Helper Script
# Quick commands to manage both Aspire and Docker Compose orchestration

param(
    [Parameter(Position=0)]
    [ValidateSet('aspire', 'docker-dev', 'docker-prod', 'docker-stop', 'docker-clean', 'status', 'help')]
    [string]$Command = 'help'
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
    Write-Host "  aspire        - Start with .NET Aspire (development)" -ForegroundColor White
    Write-Host "  docker-dev    - Start with Docker Compose (development mode)" -ForegroundColor White
    Write-Host "  docker-prod   - Start with Docker Compose (production mode)" -ForegroundColor White
    Write-Host "  docker-stop   - Stop Docker Compose services" -ForegroundColor White
    Write-Host "  docker-clean  - Stop services and remove volumes (clean slate)" -ForegroundColor White
    Write-Host "  status        - Show status of Docker services" -ForegroundColor White
    Write-Host "  help          - Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Green
    Write-Host "  .\orchestrate.ps1 aspire" -ForegroundColor Gray
    Write-Host "  .\orchestrate.ps1 docker-dev" -ForegroundColor Gray
    Write-Host "  .\orchestrate.ps1 docker-stop" -ForegroundColor Gray
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
    
    # Check if .env exists, if not create from example
    if (-not (Test-Path "$RootPath\.env")) {
        Write-Host ".env file not found. Creating from .env.example..." -ForegroundColor Yellow
        Copy-Item "$RootPath\.env.example" "$RootPath\.env"
    }
    
    Set-Location $RootPath
    docker-compose up --build
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
    docker-compose -f docker-compose.yml up --build -d
    
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
    'aspire'        { Start-Aspire }
    'docker-dev'    { Start-DockerDev }
    'docker-prod'   { Start-DockerProd }
    'docker-stop'   { Stop-Docker }
    'docker-clean'  { Clean-Docker }
    'status'        { Show-Status }
    'help'          { Show-Help }
    default         { Show-Help }
}
