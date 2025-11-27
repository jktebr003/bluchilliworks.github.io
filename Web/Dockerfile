# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BluChilliWorks.sln", "./"]
COPY ["Web/Web.csproj", "Web/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "Web/Web.csproj"

# Copy remaining source code
COPY Web/ Web/
COPY Shared/ Shared/
COPY ServiceDefaults/ ServiceDefaults/

# Build and publish
WORKDIR /src/Web
RUN dotnet build "Web.csproj" -c Release -o /app/build
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN useradd -m -u 1000 appuser && \
    chown -R appuser:appuser /app

# Copy published application
COPY --from=build /app/publish .

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "Web.dll"]
