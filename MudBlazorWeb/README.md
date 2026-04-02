# MudBlazorWeb Project

## Overview

MudBlazorWeb is a modern web application built using Blazor WebAssembly and .NET 9. It leverages MudBlazor for UI components and follows a simplified Vertical Slice Architecture combined with Clean Architecture principles. The project is designed to be modular, with each feature encapsulated in its own vertical slice for better maintainability and scalability.

The application includes features such as authentication, user management, posts, pricing, audits, and more, all organized into self-contained feature folders.

## Architecture

The project adopts a **Simplified Vertical Slice Architecture** with Clean Architecture elements:

- **Vertical Slices**: Each feature (e.g., Authentication, Posts, Pricing) is self-contained with its own Application, Domain, Infrastructure, UI, and Tests layers.
- **Clean Architecture Principles**: Separation of concerns with Domain at the center, surrounded by Application, Infrastructure, and UI layers.
- **Key Simplifications**:
  - Merged API and Application layers: DTOs, commands, queries, and handlers reside in the Application folder.
  - Fluxor state management directly in the UI folder.
  - Components flattened in the UI folder.
  - Single DependencyInjection.cs file per feature.
  - Tests co-located with features.

This structure reduces folder depth while maintaining clear boundaries and testability.

## Folder Structure

```
MudBlazorWeb/
├── Components/                 # Shared Blazor components
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/                  # Default Blazor pages
│       ├── Counter.razor
│       ├── Error.razor
│       ├── Home.razor
│       └── Weather.razor
├── Features/                   # Vertical slices for each feature
│   ├── Audits/
│   │   ├── Application/        # Commands, Queries, Handlers, DTOs, Endpoints
│   │   ├── Domain/             # Entities, Value Objects, Interfaces
│   │   ├── Infrastructure/     # Repository implementations
│   │   ├── Tests/              # Unit and integration tests
│   │   ├── UI/                 # Blazor components, Fluxor state
│   │   └── DependencyInjection.cs
│   ├── Authentication/         # Similar structure
│   ├── Contact/
│   ├── Home/
│   ├── Posts/
│   ├── Pricing/
│   ├── Users/
│   └── UserSessions/
├── Filters/                    # ASP.NET Core filters
├── Infrastructure/             # Cross-cutting concerns
│   ├── Database/               # Database setup (Postgres)
│   ├── Telemetry/              # Logging and monitoring
│   └── WebApiClient.cs
├── Shared/                     # Shared utilities and services
│   ├── Components/             # Reusable UI components
│   ├── Enums/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Helpers/
│   ├── Models/
│   └── Services/
├── wwwroot/                    # Static assets (CSS, JS, images)
├── appsettings.json            # Configuration
├── Dockerfile                  # Docker containerization
├── docker-compose.yml          # Docker Compose setup
├── Program.cs                  # Application entry point
└── MudBlazorWeb.csproj         # Project file
```

### Feature Structure Example (Pricing)

Each feature follows this pattern:

```
Features/Pricing/
├── Application/                # Business logic layer
│   ├── GetPricingsQuery.cs     # Query + Handler
│   ├── CreatePricingCommand.cs # Command + Handler
│   ├── PricingEndpoints.cs     # Minimal API endpoints
│   └── PricingDto.cs           # Data Transfer Objects
├── Domain/                     # Domain entities and rules
│   ├── Pricing.cs
│   └── IPricingRepository.cs
├── Infrastructure/             # Data access implementations
│   └── PricingRepository.cs
├── UI/                         # User interface layer
│   ├── Index.razor             # Main component
│   ├── PricingCard.razor
│   ├── PricingForm.razor
│   ├── PricingState.cs         # Fluxor state
│   ├── PricingActions.cs
│   ├── PricingReducers.cs
│   └── PricingEffects.cs
├── Tests/                      # Test files
│   ├── PricingHandlerTests.cs
│   └── PricingReducerTests.cs
├── DependencyInjection.cs      # Feature-specific DI configuration
└── README.md                   # Feature documentation
```

## Technologies

- **Framework**: .NET 9, Blazor WebAssembly
- **UI**: MudBlazor (Material Design components)
- **State Management**: Fluxor (Redux pattern for Blazor)
- **Architecture**: MediatR (CQRS pattern), Vertical Slice Architecture
- **Data Access**: Dapper, Entity Framework Core
- **Database**: PostgreSQL
- **Validation**: FluentValidation
- **API Versioning**: ASP.Versioning
- **Background Jobs**: Hangfire
- **Logging**: log4net
- **Email**: MailKit
- **Mapping**: Mapster
- **Containerization**: Docker, Docker Compose

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- PostgreSQL (or use Docker)

### Running the Application

1. **Clone the repository** and navigate to the project directory.

2. **Build and run with Docker Compose**:
   - For debug mode: `docker-compose -f docker-compose.yml -f docker-compose.debug.yml up --build -d`
   - For release mode: `docker-compose -f docker-compose.yml up --build -d`

3. **Stop the application**:
   - `docker-compose -f docker-compose.yml -f docker-compose.debug.yml down --remove-orphans`

4. **Push images** (if needed):
   - `docker-compose -f docker-compose.yml push`

### Development

- Open the solution in Visual Studio or VS Code.
- Restore NuGet packages: `dotnet restore`
- Run the application: `dotnet run`
- For debugging, use the launch settings in `Properties/launchSettings.json`.

### Testing

Run tests for individual features or the entire project:

```bash
dotnet test
```

## Contributing

- Each feature is self-contained; add new features following the established vertical slice pattern.
- Ensure tests are included for new functionality.
- Follow the dependency injection pattern in `DependencyInjection.cs` files.

## License

[Specify license if applicable]