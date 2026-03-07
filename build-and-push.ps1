param(
    [Parameter(Mandatory=$true)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "latest",
    
    [switch]$SkipBuild,
    [switch]$ApiOnly,
    [switch]$WebOnly
)

$ErrorActionPreference = "Stop"

# Configuration
$ApiImageName = "bluchilliworks-api"
$WebImageName = "bluchilliworks-web"

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Docker Build and Push Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Green
Write-Host "  Docker Hub Username: $Username" -ForegroundColor White
Write-Host "  Version Tag: $Version" -ForegroundColor White
Write-Host "  Skip Build: $SkipBuild" -ForegroundColor White
Write-Host "  API Only: $ApiOnly" -ForegroundColor White
Write-Host "  Web Only: $WebOnly" -ForegroundColor White
Write-Host ""

# Determine which images to process
$buildApi = -not $WebOnly
$buildWeb = -not $ApiOnly

# Build Docker images
if (-not $SkipBuild) {
    Write-Host "Building Docker images..." -ForegroundColor Yellow
    Write-Host ""
    
    if ($buildApi) {
        Write-Host "Building API image..." -ForegroundColor Cyan
        docker build -t ${Username}/${ApiImageName}:${Version} -t ${Username}/${ApiImageName}:latest -f Api/Dockerfile.windows .
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build API image" -ForegroundColor Red
            exit 1
        }
        Write-Host "API image built successfully" -ForegroundColor Green
        Write-Host ""
    }
    
    if ($buildWeb) {
        Write-Host "Building Web image..." -ForegroundColor Cyan
        docker build -t ${Username}/${WebImageName}:${Version} -t ${Username}/${WebImageName}:latest -f Web/Dockerfile.windows .
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build Web image" -ForegroundColor Red
            exit 1
        }
        Write-Host "Web image built successfully" -ForegroundColor Green
        Write-Host ""
    }
} else {
    Write-Host "Skipping build step..." -ForegroundColor Yellow
    Write-Host ""
}

# Login to Docker Hub
Write-Host "Logging in to Docker Hub..." -ForegroundColor Yellow
docker login -u $Username
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to login to Docker Hub" -ForegroundColor Red
    exit 1
}
Write-Host "Logged in successfully" -ForegroundColor Green
Write-Host ""

# Push images to Docker Hub
Write-Host "Pushing images to Docker Hub..." -ForegroundColor Yellow
Write-Host ""

if ($buildApi) {
    Write-Host "Pushing API image (version: $Version)..." -ForegroundColor Cyan
    docker push ${Username}/${ApiImageName}:${Version}
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to push API image with version tag" -ForegroundColor Red
        exit 1
    }
    
    if ($Version -ne "latest") {
        Write-Host "Pushing API image (latest)..." -ForegroundColor Cyan
        docker push ${Username}/${ApiImageName}:latest
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to push API image with latest tag" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "API image pushed successfully" -ForegroundColor Green
    Write-Host "  Image: ${Username}/${ApiImageName}:${Version}" -ForegroundColor White
    if ($Version -ne "latest") {
        Write-Host "  Image: ${Username}/${ApiImageName}:latest" -ForegroundColor White
    }
    Write-Host ""
}

if ($buildWeb) {
    Write-Host "Pushing Web image (version: $Version)..." -ForegroundColor Cyan
    docker push ${Username}/${WebImageName}:${Version}
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to push Web image with version tag" -ForegroundColor Red
        exit 1
    }
    
    if ($Version -ne "latest") {
        Write-Host "Pushing Web image (latest)..." -ForegroundColor Cyan
        docker push ${Username}/${WebImageName}:latest
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to push Web image with latest tag" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "Web image pushed successfully" -ForegroundColor Green
    Write-Host "  Image: ${Username}/${WebImageName}:${Version}" -ForegroundColor White
    if ($Version -ne "latest") {
        Write-Host "  Image: ${Username}/${WebImageName}:latest" -ForegroundColor White
    }
    Write-Host ""
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Green
Write-Host "Build and Push Completed!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "Your images are now available on Docker Hub:" -ForegroundColor White

if ($buildApi) {
    Write-Host "  API: https://hub.docker.com/r/${Username}/${ApiImageName}" -ForegroundColor Cyan
}
if ($buildWeb) {
    Write-Host "  Web: https://hub.docker.com/r/${Username}/${WebImageName}" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "To pull your images:" -ForegroundColor White
if ($buildApi) {
    Write-Host "  docker pull ${Username}/${ApiImageName}:${Version}" -ForegroundColor Yellow
}
if ($buildWeb) {
    Write-Host "  docker pull ${Username}/${WebImageName}:${Version}" -ForegroundColor Yellow
}
Write-Host ""
