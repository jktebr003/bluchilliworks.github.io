# BluChilli Works

A modern full-stack application built with .NET 9, Blazor WebAssembly, and MongoDB.

## 🚀 Quick Start

### Option 1: .NET Aspire (Recommended for Development)
```powershell
.\orchestrate.ps1 aspire
```

### Option 2: Docker Compose (Deployment)
```powershell
.\orchestrate.ps1 docker-dev
```

See **[QUICKSTART.md](QUICKSTART.md)** for detailed instructions.

## 📚 Documentation

- **[QUICKSTART.md](QUICKSTART.md)** - Get started in 2 minutes
- **[ORCHESTRATION.md](ORCHESTRATION.md)** - Complete orchestration guide
- **[IMPLEMENTATION-SUMMARY.md](IMPLEMENTATION-SUMMARY.md)** - Technical implementation details

## 🏗️ Architecture

```
├── Api/                    # ASP.NET Core Web API
│   ├── Features/          # Feature-based organization
│   ├── Services/          # Application services
│   └── Dockerfile         # Container image definition
├── Web/                    # Blazor WebAssembly frontend
│   ├── Features/          # Feature-based organization
│   ├── Pages/             # Blazor pages
│   └── Dockerfile         # Container image definition
├── AppHost/               # .NET Aspire orchestration
├── Shared/                # Shared models and utilities
└── ServiceDefaults/       # Common service configuration
```

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET platform
- **Blazor WebAssembly** - Modern SPA framework
- **ASP.NET Core Web API** - RESTful API
- **MongoDB** - NoSQL database
- **Hangfire** - Background job processing
- **.NET Aspire** - Cloud-native orchestration
- **Docker** - Containerization
- **MediatR** - CQRS pattern
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

## 🔧 Development Tools

### Helper Script
Use the PowerShell helper script for common tasks:
```powershell
.\orchestrate.ps1 [command]

Commands:
  aspire        - Start with .NET Aspire
  docker-dev    - Start Docker (development)
  docker-prod   - Start Docker (production)
  docker-stop   - Stop Docker services
  docker-clean  - Clean everything
  status        - Show service status
  help          - Show help
```

### Manual Commands

**Build Solution:**
```powershell
dotnet build BluChilliWorks.sln
```

**Run Tests:**
```powershell
dotnet test
```

**Database Connection:**
```
mongodb://admin:Admin123!@localhost:27017
```

## 🌐 Service URLs

### .NET Aspire
- **Dashboard:** http://localhost:15000 (auto-opens)
- **API:** Dynamic port (check dashboard)
- **Web:** Dynamic port (check dashboard)

### Docker Compose
- **API:** http://localhost:5000
- **Web:** http://localhost:5001
- **MongoDB:** localhost:27017
- **API Health:** http://localhost:5000/health
- **Swagger:** http://localhost:5000/swagger

## 📦 Features

- **Authentication & Authorization** - API key-based auth
- **Email Service** - Template-based email sending
- **Background Jobs** - Hangfire-based job processing
- **File Management** - Package and document handling
- **Real-time Messaging** - Message queue system
- **API Versioning** - Backward compatibility
- **Swagger Documentation** - Interactive API docs
- **Health Checks** - Service monitoring endpoints

## 🔒 Security

- Non-root container users
- Environment-based secrets
- API key authentication
- Network isolation
- MongoDB authentication
- Input validation

## 📝 Configuration

### Environment Variables
Copy `.env.example` to `.env` and configure:
- MongoDB credentials
- API keys
- SMTP settings

See `.env.production.template` for production configuration.

## 🐛 Troubleshooting

### Port Already in Use
```powershell
netstat -ano | findstr :5000
taskkill /PID <pid> /F
```

### MongoDB Connection Issues
Check if MongoDB is running:
```powershell
docker-compose ps
```

### Build Issues
Clean and rebuild:
```powershell
dotnet clean
dotnet build
```

See **[ORCHESTRATION.md](ORCHESTRATION.md)** for more troubleshooting tips.

## 📊 Project Structure

- **Feature-based organization** - Related files grouped by feature
- **Clean architecture** - Separation of concerns
- **CQRS pattern** - Commands and queries separated
- **Repository pattern** - Data access abstraction
- **Dependency injection** - Loose coupling

## 🚢 Deployment

### Docker Compose (Simple)
```powershell
.\orchestrate.ps1 docker-prod
```

### Container Registries
```powershell
docker build -f Api/Dockerfile -t your-registry/bluchilli-api:latest .
docker push your-registry/bluchilli-api:latest
```

See **[ORCHESTRATION.md](ORCHESTRATION.md)** deployment section for details.

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test with both Aspire and Docker
4. Submit a pull request

## 📄 License

[Your License Here]

## 🆘 Support

- Check documentation in `ORCHESTRATION.md`
- Review health checks: `http://localhost:5000/health`
- View logs: `docker-compose logs -f` or Aspire dashboard

---

**Built with ❤️ using .NET 9 and Aspire**
