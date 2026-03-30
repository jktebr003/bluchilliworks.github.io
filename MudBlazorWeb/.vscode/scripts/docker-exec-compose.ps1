param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ForwardedArgs
)

$ErrorActionPreference = 'Stop'

# Resolve the running container ID for the compose service.
$containerId = docker compose -f docker-compose.yml -f docker-compose.debug.yml ps -q mudblazorweb
if (-not $containerId) {
    $containerId = docker compose -f docker-compose.yml ps -q mudblazorweb
}

if (-not $containerId) {
    throw "No running container found for compose service 'mudblazorweb'. Start it first with a docker compose up task."
}

# Execute the debugger command inside the resolved container.
& docker exec -i $containerId @ForwardedArgs
exit $LASTEXITCODE
