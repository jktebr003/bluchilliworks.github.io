# Contributing Guidelines: Vertical Slice + Clean Architecture per Feature

## Purpose
This repository adopts a Vertical Slice architecture using a top-level `Features/` folder. Each feature is a self-contained unit that implements Clean Architecture principles (separation of API/Contracts, Application logic, Domain models, Infrastructure, and UI) inside the feature. These guidelines describe how to add, structure, name, test, and review features so the codebase remains consistent and maintainable.

## High-level principles
- Feature-first: Organize by feature, not technical layer. Each feature contains its own Clear Architecture boundaries.
- Explicit boundaries: Keep Domain models and business rules independent from Infrastructure and UI.
- Small, focused commits and PRs. Each PR should represent a single feature or a small set of related changes.
- Tests live alongside the feature (unit + integration/acceptance test projects) and are named consistently.

## Folder layout (recommended)
Each feature lives under `src/Features/<FeatureName>/` for library code and `src/Web/Client/Features/<FeatureName>/` for Blazor client UI when needed.

Example feature tree:

- src/
  - Features/
    - Orders/
      - Api/                  # API endpoints, Contracts, DTOs
        - CreateOrder.cs
        - CreateOrderResponse.cs
      - Application/          # Use cases, handlers, services
        - CreateOrderHandler.cs
        - CreateOrderCommand.cs
      - Domain/               # Entities, ValueObjects, Domain Events
        - Order.cs
        - OrderId.cs
        - IOrderRepository.cs
      - Infrastructure/       # EF Core, Repositories, Adapters
        - OrderRepository.cs
      - UI/                   # Blazor components or Razor Views specific to feature
        - Components/
          - OrderList.razor
          - OrderDetail.razor
      - Tests/                # Unit and integration tests for the feature
        - Orders.Application.Tests.csproj
        - CreateOrderHandlerTests.cs

Notes:
- Keep public APIs for the feature minimal and expressed via Contracts/DTOs inside `Api/`.
- Keep feature-level dependencies to a minimum; prefer application-wide DI registration extension methods.

## Naming conventions
- Feature folder names are PascalCase and singular where appropriate (e.g., `Orders`, `Account`).
- Handlers/UseCases follow `VerbNounHandler` or `VerbNounCommand` naming (e.g., `CreateOrderHandler`, `GetOrderQuery`).
- DTOs/Requests/Responses are suffixed with `Request`/`Response` or `Dto` when used across boundaries (e.g., `CreateOrderRequest`, `OrderDto`).
- Domain entities use domain language and do not include suffixes like `Entity` unless ambiguous.

## Clean Architecture within a feature
- Api/Contracts: Public contracts — DTOs, minimal request/response models, endpoint routes.
- Application: Use-cases, request/response messages, handlers, validation, mapping.
- Domain: Pure domain models, value objects, domain exceptions, domain events.
- Infrastructure: Persistence, external integrations, repository implementations, proxies. Implementation details depend on IoC and should be injected via interfaces defined in Application or Domain.
- UI: Feature-specific UI components (Blazor) or pages.

Dependency rule (inside one feature): Domain <- Application <- Api/UI. Infrastructure depends on Domain/Application but not vice-versa.

## Dependency injection
- Each feature exposes a single extension method for DI, e.g. `public static IServiceCollection AddOrdersFeature(this IServiceCollection services)` located in `Infrastructure/DependencyInjection.cs` or `Application/DependencyInjection.cs` depending on what registrations are required.
- Application-level registrations (cross-feature) belong in the top-level `src/Web/Server/Startup` or `Program.cs` and should call each feature's `AddXFeature`.
- Avoid sprinkling registrations across the solution.

Example registration pattern:

- src/Features/Orders/Infrastructure/DependencyInjection.cs
  - `public static IServiceCollection AddOrdersFeature(this IServiceCollection services, IConfiguration configuration) { ... }`

## Using MediatR or explicit handlers
- Either pattern is acceptable—use MediatR for simpler wiring of requests/handlers, or explicit interface handlers for clarity.
- If using MediatR, keep pipeline behaviors (validation, logging) centralized.

## Blazor (WASM + Server) specifics
- UI components for a feature live under `src/Web/Client/Features/<FeatureName>/Components`.
- Shared UI artifacts (layouts, shells) live under `src/Web/Client/Shared`.
- Prefer small components and single-purpose pages per feature.
- Feature-specific CSS/SCSS may live adjacent to components.

## Testing
- Unit tests live under `src/Features/<FeatureName>/Tests` or `tests/Features/<FeatureName>.UnitTests` following the repository test structure.
- Integration tests that exercise multiple features may live in a separate `tests/Integration` project.
- Naming: `<UnitUnderTest>Tests` or `<Feature>.<Concern>Tests` (e.g., `CreateOrderHandlerTests`).
- CI must run unit and integration tests on PRs.

## Adding a new feature (checklist)
1. Create folder `src/Features/<FeatureName>/` with the subfolders: `Api`, `Application`, `Domain`, `Infrastructure`, `UI`, `Tests`.
2. Add a `DependencyInjection` extension to register the feature.
3. Add minimal public contract(s) in `Api/` and a single entry handler in `Application/` to prove the feature works.
4. Wire DI by calling `Add<FeatureName>Feature` in `Program.cs`.
5. Add unit tests in the feature `Tests/` folder.
6. Add README.md inside the feature folder describing responsibilities, public endpoints, and any migration steps.

## PR checklist
- [ ] PR is limited in scope and describes the feature clearly.
- [ ] Naming, folder layout and dependency rules are followed.
- [ ] Tests added or updated and all tests pass locally.
- [ ] Feature DI added and `Program.cs` changes minimal.
- [ ] No domain logic in Infrastructure or UI.

## Commit message guidelines
- Use present-tense, imperative style: "Add CreateOrder handler".
- Group related changes; do not include unrelated refactors.

## Tooling / Formatting
- This repository uses an `.editorconfig` file. Keep code format in sync with that file.
- Run `dotnet format` before committing to normalize whitespaces and styles.

## Rationale and guidance
- The vertical-slice approach reduces cognitive load when working on a single feature and improves encapsulation for the team.
- Clean Architecture inside each feature keeps domain rules isolated and easier to test.
- Keeping DI registration per feature enables modular composition and easier feature extraction.

## Example minimal feature: CreateOrder
- `Api/CreateOrderRequest.cs`
- `Application/CreateOrderCommand.cs`
- `Application/CreateOrderHandler.cs`
- `Domain/Order.cs`
- `Infrastructure/OrderRepository.cs`
- `Infrastructure/DependencyInjection.cs`
- `UI/Components/CreateOrderForm.razor`
- `Tests/CreateOrderHandlerTests.cs`

## FAQ
Q: When should shared code go into a separate project?
A: When multiple features need the same functionality (e.g., shared DTOs, cross-cutting services), create a small, well-scoped shared project (e.g., `src/Shared/` or `src/Kernel/`) with clear responsibilities.

Q: How to handle cross-feature transactions?
A: Prefer orchestrators in an Application-level service or a dedicated cross-feature application layer. Keep domain logic inside respective Domain folders and orchestrate via Application services or integration events.

---

Thank you for following these guidelines. The next suggested steps are:
- Create the `CONTRIBUTING.md` file in the repository root with these contents (this will be applied now).
- Add an `.editorconfig` with formatting rules tailored to .NET 9 and Blazor projects (can be provided on request).