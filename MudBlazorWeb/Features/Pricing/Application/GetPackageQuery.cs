using Carter;

using Mapster;

using MediatR;

using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Pricing.Application;

public static class GetPackageQuery
{
    // DTO
    public record PackageDto(string Name, string Description, string Code, string Price, PackageType PackageType, bool ShowPackage) : BaseAuditableDto
    {
        // Factory method to create from domain entity
        public static PackageDto FromEntity(Package package)
        {
            return new PackageDto(
                package.Name,
                package.Description,
                package.Code,
                package.Price,
                (PackageType)package.Type,
                true
            )
            {
                Id = package.Id,
                CreatedOn = package.CreatedOn,
                CreatedBy = package.CreatedBy,
                ModifiedOn = package.ModifiedOn,
                ModifiedBy = package.ModifiedBy,
                DeletedOn = package.DeletedOn,
                DeletedBy = package.DeletedBy,
                IsDeleted = package.IsDeleted
            };
        }
    }

    // Query
    public record Query(Guid Id) : IRequest<Result<PackageDto>>;

    // Handler
    internal sealed class Handler : IRequestHandler<Query, Result<PackageDto>>
    {
        private readonly IPackageRepository _packageRepository;

        public Handler(IPackageRepository packageRepository) => _packageRepository = packageRepository;

        public async Task<Result<PackageDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var data = await _packageRepository.GetPackageByIdAsync(request.Id);
            if (data == null)
            {
                return new Result<PackageDto>(PackageDtoEmpty.Instance, false, "GetPackage.Null", "The package with the specified ID was not found");
            }

            // Use factory method for clean mapping
            return new Result<PackageDto>(PackageDto.FromEntity(data), true);
        }

        // Static empty response to avoid unnecessary allocations
        private sealed record PackageDtoEmpty : PackageDto
        {
            public static readonly PackageDtoEmpty Instance = new();
            private PackageDtoEmpty() : base(string.Empty, string.Empty, string.Empty, string.Empty, default, false) { }
        }
    }
}

public class GetPackageQueryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("packages/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetPackageQuery.Query(id);

            var result = await sender.Send(query);

            return Results.Ok(result);
        }).WithTags("Packages")
          .AddEndpointFilter<AuthenticationFilter>(); ;
    }
}
