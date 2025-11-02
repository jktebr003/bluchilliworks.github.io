using Api.Filters;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Packages;

public static class GetPackage
{
    public class Query : IRequest<ApiResult<PackageResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<PackageResponse>>
    {
        private readonly IPackageRepository _packageRepository;

        public Handler(IPackageRepository packageRepository) => _packageRepository = packageRepository;

        public async Task<ApiResult<PackageResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Remove ConfigureAwait(false) for ASP.NET Core context consistency
            var data = await _packageRepository.GetPackageByIdAsync(request.Id);
            if (data == null)
            {
                // Use static empty response to avoid repeated allocations
                return new ApiResult<PackageResponse>(PackageResponseEmpty.Instance, false, "GetPackage.Null", "The package with the specified ID was not found");
            }

            // Use object initializer directly in return for clarity and efficiency
            return new ApiResult<PackageResponse>(
                new PackageResponse
                {
                    ID = data.ID,
                    Code = data.Code,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    DeletedBy = data.DeletedBy,
                    DeletedOn = data.DeletedOn,
                    Description = data.Description,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedOn = data.ModifiedOn,
                    Name = data.Name,
                    Price = data.Price,
                    Type = (Shared.Enums.PackageType)data.Type
                },
                true
            );
        }

        // Static empty response to avoid unnecessary allocations
        private sealed class PackageResponseEmpty : PackageResponse
        {
            public static readonly PackageResponseEmpty Instance = new();
            private PackageResponseEmpty() { }
        }
    }
}

public class GetPackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("packages/{id}", async (string id, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new GetPackage.Query { Id = id });
            return Results.Ok(result);
        })
        .WithTags("Packages")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}

