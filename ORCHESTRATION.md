# BluChilli Works - Container Orchestration

This project supports **two orchestration approaches** for maximum flexibility:
- **.NET Aspire** - Modern development with built-in observability
- **Docker Compose** - Traditional containerization for deployment

---

## 🚀 Quick Start

### Option 1: .NET Aspire (Recommended for Development)

**Prerequisites:**
- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code with C# Dev Kit

**Run the application:**
```powershell
cd AppHost
dotnet restore
dotnet run
```

The Aspire dashboard will launch automatically at `http://localhost:15000` (or similar), providing:
- Real-time service status
- Distributed tracing
- Logs aggregation
- Metrics visualization

**Services:**
- API: `http://localhost:5000` (or dynamic port)
- Web: `http://localhost:5001` (or dynamic port)
- MongoDB: Automatically provisioned in Docker container

---

### Option 2: Docker Compose (Production & Deployment)

**Prerequisites:**
- Docker Desktop
- Docker Compose v2+

**Development Mode (with hot reload):**
```powershell
docker-compose up --build
```

**Production Mode:**
```powershell
docker-compose -f docker-compose.yml up --build -d
```

**Stop services:**
```powershell
docker-compose down
```

**Stop and remove volumes (clean start):**
```powershell
docker-compose down -v
```

**Services:**
- API: `http://localhost:5000`
- Web: `http://localhost:5001`
- MongoDB: `localhost:27017`

---

## 📦 Architecture Overview

### Project Structure
```
bluchilliworks.github.io/
├── Api/                    # ASP.NET Core Web API
│   ├── Dockerfile         # Multi-stage API build
│   └── Program.cs
├── Web/                    # Blazor WebAssembly
│   └── Dockerfile         # Multi-stage Web build
├── AppHost/               # .NET Aspire orchestration
│   └── AppHost.cs
├── Shared/                # Shared models/utilities
├── ServiceDefaults/       # Common service configuration
├── docker-compose.yml     # Production Docker config
└── docker-compose.override.yml  # Development overrides
```

### Service Dependencies
```
MongoDB (Container)
    ↓
API (Depends on MongoDB)
    ↓
Web (Depends on API)
```

---

## 🔧 Configuration

### Environment Variables

Create a `.env` file in the root directory for Docker Compose:

```env
# MongoDB
MONGO_ROOT_PASSWORD=YourSecurePassword123!

# API Keys
API_KEY=your-api-key-here
API_KEY_SECONDARY=your-secondary-api-key-here

# Email Configuration
EMAIL_FROM=noreply@bluchilliworks.com
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USERNAME=your-smtp-username
SMTP_PASSWORD=your-smtp-password
```

### Aspire Configuration

Aspire uses `appsettings.json` and `appsettings.Development.json` in each project.
MongoDB connection is automatically configured via service references.

### Docker Compose Configuration

**Development:** Uses `docker-compose.override.yml` (auto-loaded)
- Hot reload enabled
- Development credentials
- Volume mounts for source code

**Production:** Uses only `docker-compose.yml`
- Optimized builds
- Secure credentials via `.env`
- No source mounts

---

## 🏥 Health Checks

Both orchestration methods include health checks:

**API Health Check:**
```
GET http://localhost:5000/health
```

**MongoDB Health:**
- Aspire: Automatic monitoring via dashboard
- Docker: `docker ps` shows health status

---

## 🔍 Monitoring & Debugging

### .NET Aspire Dashboard

When using Aspire, the dashboard provides:
- **Traces:** Distributed tracing across services
- **Metrics:** Performance counters and custom metrics
- **Logs:** Centralized logging from all services
- **Resources:** Service status and dependencies

Access at: `http://localhost:15000` (check console output for exact URL)

### Docker Compose Logs

```powershell
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f mongodb

# View last 100 lines
docker-compose logs --tail=100 api
```

---

## 🛠️ Development Workflows

### Using Aspire (Development)

1. **Start all services:**
   ```powershell
   cd AppHost
   dotnet run
   ```

2. **Make code changes** - Aspire detects changes and hot-reloads

3. **View logs** - Check Aspire dashboard for real-time logs

4. **Debug** - Attach debugger to individual services in Visual Studio

### Using Docker Compose (Development)

1. **Start with hot reload:**
   ```powershell
   docker-compose up
   ```

2. **Make code changes** - Files are mounted, `dotnet watch` auto-reloads

3. **View logs:**
   ```powershell
   docker-compose logs -f api
   ```

4. **Rebuild after dependency changes:**
   ```powershell
   docker-compose up --build
   ```

---

## 📊 Database Management

### MongoDB Access

**Via Docker Compose:**
```powershell
# Connect with mongosh
docker-compose exec mongodb mongosh -u admin -p Admin123!

# Or use MongoDB Compass
# Connection string: mongodb://admin:Admin123!@localhost:27017
```

**Via Aspire:**
MongoDB runs in a container managed by Aspire. Connection details are in the Aspire dashboard.

### Database Migrations

The API automatically initializes MongoDB collections on first run via `MongoDB.Entities`.

---

## 🚢 Deployment

### Deployment Strategies

**1. Docker Compose (Simple deployments):**
```powershell
# On production server
docker-compose -f docker-compose.yml up -d

# Update environment variables in .env file first
```

**2. Kubernetes (Scalable deployments):**
Convert Docker Compose to Kubernetes manifests using tools like Kompose.

**3. Azure Container Apps / AWS ECS:**
Build and push images, then deploy using platform-specific tools.

### Building Production Images

```powershell
# Build API image
docker build -f Api/Dockerfile -t bluchilli-api:latest .

# Build Web image
docker build -f Web/Dockerfile -t bluchilli-web:latest .

# Tag and push to registry
docker tag bluchilli-api:latest your-registry.azurecr.io/bluchilli-api:latest
docker push your-registry.azurecr.io/bluchilli-api:latest
```

---

## 🧪 Testing

### Run Tests
```powershell
dotnet test BluChilliWorks.sln
```

### Integration Tests with Docker
```powershell
# Start dependencies only
docker-compose up -d mongodb

# Run tests
dotnet test

# Cleanup
docker-compose down
```

---

## 🔒 Security Best Practices

1. **Never commit secrets** - Use `.env` files (add to `.gitignore`)
2. **Rotate API keys** - Change default keys in production
3. **Strong MongoDB password** - Use complex passwords in production
4. **HTTPS in production** - Configure reverse proxy (nginx/traefik)
5. **Network isolation** - Services communicate via internal Docker network
6. **Non-root containers** - Dockerfiles use non-root user `appuser`

---

## 📝 Common Commands

### Aspire
```powershell
# Run with specific environment
dotnet run --project AppHost --environment Production

# Clean and rebuild
dotnet clean
dotnet build
```

### Docker Compose
```powershell
# Start in detached mode
docker-compose up -d

# Restart single service
docker-compose restart api

# View resource usage
docker stats

# Remove all containers and volumes
docker-compose down -v --remove-orphans

# Pull latest images
docker-compose pull
```

---

## 🐛 Troubleshooting

### Issue: Port already in use
```powershell
# Find process using port
netstat -ano | findstr :5000

# Kill process (replace PID)
taskkill /PID <pid> /F
```

### Issue: MongoDB connection fails
- Check if MongoDB container is healthy: `docker ps`
- Verify connection string in configuration
- Ensure MongoDB container started before API

### Issue: Docker build fails
- Clear Docker cache: `docker builder prune`
- Rebuild without cache: `docker-compose build --no-cache`
- Check available disk space

### Issue: Aspire dashboard not opening
- Check console output for correct URL
- Ensure port 15000 (or assigned port) is not blocked
- Restart Aspire: Stop and run `dotnet run` again

---

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Hangfire Documentation](https://docs.hangfire.io/)

---

## 🤝 Contributing

When adding new services:

1. **Update AppHost.cs** - Add service references in Aspire
2. **Update docker-compose.yml** - Add service definitions
3. **Create Dockerfile** - Follow multi-stage build pattern
4. **Update this README** - Document new services

---

## 📄 License

[Your License Here]

---

## ✅ Checklist for Production Deployment

- [ ] Update MongoDB password in `.env`
- [ ] Change API keys from defaults
- [ ] Configure production SMTP credentials
- [ ] Set up HTTPS/TLS certificates
- [ ] Configure health monitoring/alerting
- [ ] Set up backup strategy for MongoDB
- [ ] Review and adjust resource limits (CPU/Memory)
- [ ] Configure log retention policies
- [ ] Set up CI/CD pipeline
- [ ] Document disaster recovery procedures
