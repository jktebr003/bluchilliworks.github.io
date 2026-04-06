# .env Setup for Docker Compose Deployment

This project uses Docker Compose variable interpolation in [docker-compose.yml](docker-compose.yml) and [docker-compose.debug.yml](docker-compose.debug.yml).

The .env file lets you define values once and reuse them for build, run, and publish commands.

## How .env works

When you run docker compose commands from this folder, Compose reads variables from a local .env file and injects them into placeholders such as:

- ${DOCKERHUB_USER:-local}
- ${IMAGE_TAG:-latest}
- ${BUILD_CONFIGURATION:-Release}
- ${ASPNETCORE_ENVIRONMENT:-Production}
- ${APP_HTTP_PORT:-8080}
- ${APP_HTTPS_PORT:-8081}

The part after :- is the default used when a variable is not set.

## Variable precedence

For Compose interpolation, values are resolved in this practical order:

1. Environment variables already set in your shell/session.
2. Values from the .env file in this folder.
3. Default values from the compose file (the :-value fallback).

This means you can keep stable values in .env and override specific values temporarily in CI/CD or a terminal session.

## Initial setup

1. Copy [.env.example](.env.example) to a new file named .env in this folder.
2. Edit .env and set deployment-specific values.
3. Keep .env out of source control if it contains sensitive values.

Example .env for production publishing:

```env
DOCKERHUB_USER=your-dockerhub-user
IMAGE_TAG=2026.03.30
BUILD_CONFIGURATION=Release
ASPNETCORE_ENVIRONMENT=Production
APP_HTTP_PORT=8080
APP_HTTPS_PORT=8081
```

## What each variable controls

- DOCKERHUB_USER: Docker Hub namespace used by the image name.
- IMAGE_TAG: Image tag used for versioning and rollbacks.
- BUILD_CONFIGURATION: Dotnet build config passed into the Docker build stage.
- ASPNETCORE_ENVIRONMENT: Runtime environment inside the container.
- APP_HTTP_PORT: Host port mapped to container port 8080.
- APP_HTTPS_PORT: Host port mapped to container port 8081 (debug override).

## Deployment workflow with .env

Run these commands from this folder after .env is configured:

1. docker login
2. docker compose -f docker-compose.yml build
3. docker compose -f docker-compose.yml push
4. docker compose -f docker-compose.yml up -d

Optional debug/development run:

1. docker compose -f docker-compose.yml -f docker-compose.debug.yml up --build -d
2. docker compose --env-file .env.testing -f docker-compose.testing.yml up --build -d

## CI/CD recommendation

In CI/CD, prefer setting variables as pipeline environment variables/secrets rather than committing a .env file.

Suggested minimum CI variables:

- DOCKERHUB_USER
- IMAGE_TAG
- BUILD_CONFIGURATION
- ASPNETCORE_ENVIRONMENT

## Common mistakes

- Running docker compose from a different folder, so .env is not discovered.
- Using a Docker Hub namespace that does not match the account used in docker login.
- Reusing latest for all deployments, which makes rollback harder.
- Exposing host ports that are already in use on the target server.
