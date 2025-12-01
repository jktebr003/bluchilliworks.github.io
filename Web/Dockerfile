# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BluChilliWorks.sln", "./"]
COPY ["Web/Web.csproj", "Web/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "Web/Web.csproj"

# Copy remaining source code
COPY Web/ Web/
COPY Shared/ Shared/

# Build and publish (Blazor WebAssembly publishes to wwwroot)
WORKDIR /src/Web
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

# Runtime stage - use nginx to serve static files
FROM nginx:alpine AS runtime

# Copy published Blazor WebAssembly files
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy custom nginx configuration
COPY --from=build /src/Web/nginx.conf /etc/nginx/nginx.conf

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1
