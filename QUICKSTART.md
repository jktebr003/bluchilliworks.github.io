# 🚀 Quick Start Guide

## Choose Your Orchestration Method

### Option 1: .NET Aspire (Development - Recommended) ⚡

```powershell
# 1. Restore AppHost dependencies
cd AppHost
dotnet restore

# 2. Run Aspire
dotnet run
```

✅ **Aspire Dashboard opens automatically**
- API, Web, and MongoDB all start together
- Real-time monitoring at http://localhost:15000
- Hot reload enabled

---

### Option 2: Docker Compose (Production & Deployment) 🐳

```powershell
# 1. Copy environment template
cp .env.example .env

# 2. Edit .env with your settings (optional for dev)

# 3. Start all services
docker-compose up --build
```

✅ **Services available at:**
- API: http://localhost:5000
- Web: http://localhost:5001
- MongoDB: localhost:27017

**Stop services:**
```powershell
docker-compose down
```

---

## 📖 Full Documentation

See **[ORCHESTRATION.md](ORCHESTRATION.md)** for complete setup, configuration, and deployment instructions.

---

## ⚡ Quick Tips

### Using Aspire
- Best for local development
- Automatic service discovery
- Built-in observability dashboard
- No manual configuration needed

### Using Docker Compose
- Best for production/deployment
- Full control over environment
- Portable across platforms
- Requires `.env` configuration

---

## 🛠️ Troubleshooting

**Port conflicts?**
```powershell
netstat -ano | findstr :5000
```

**Need to reset?**
```powershell
# Aspire: Just restart
# Docker: Remove volumes
docker-compose down -v
```

**Build issues?**
```powershell
# Clean Docker cache
docker builder prune

# Rebuild without cache
docker-compose build --no-cache
```

---

## 📚 Next Steps

1. ✅ Choose your orchestration method above
2. 📖 Read [ORCHESTRATION.md](ORCHESTRATION.md) for details
3. 🔧 Configure environment variables in `.env`
4. 🚀 Start developing!
