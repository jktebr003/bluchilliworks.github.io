# Container Workflow (Visual Studio + VS Code)

This project uses Windows containers and the Dockerfile in this folder.

For environment-variable configuration and deployment setup details, see [ENV-SETUP.md](ENV-SETUP.md).

## Prerequisites

- Docker Desktop set to Windows containers.
- Sign in to Docker Hub:
  - `docker login`

## Build, run, and publish with Docker Compose

From this folder:

- Build and run (release):
  - `docker compose -f docker-compose.yml up --build -d`
- Build and run (debug/development):
  - `docker compose -f docker-compose.yml -f docker-compose.debug.yml up --build -d`
- View logs:
  - `docker compose -f docker-compose.yml logs -f`
- Push to Docker Hub:
  - `set DOCKERHUB_USER=<your-dockerhub-user>`
  - `set IMAGE_TAG=latest`
  - `docker compose -f docker-compose.yml build`
  - `docker compose -f docker-compose.yml push`
- Stop containers:
  - `docker compose -f docker-compose.yml -f docker-compose.debug.yml down --remove-orphans`

## Visual Studio

- Continue using the existing launch profile `Container (Dockerfile)` in launch settings.
- Visual Studio container debugging works directly with this Dockerfile.
- Compose can also be run from Terminal if desired.

## VS Code

- Tasks are provided in `.vscode/tasks.json`:
  - `docker-compose: up (debug)`
  - `docker-compose: up (release)`
  - `docker-compose: down`
  - `docker-compose: push`
- A launch config is provided in `.vscode/launch.json` for attaching to the running container process.
- Container detection is automatic via `.vscode/scripts/docker-exec-compose.ps1`.

### Note about the attach configuration

The attach profile resolves the running container ID using `docker compose ps -q mudblazorweb`.
If no container is running, start one first using the debug or release compose task.
