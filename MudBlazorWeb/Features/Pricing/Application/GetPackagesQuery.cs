using Carter;

using MediatR;

using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Pricing.Application;

public static class GetPackagesQuery
{
    // DTO (reuse from GetPackageQuery)
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
    public record Query(int? PageSize, int? PageNumber) : IRequest<PagedResult<List<PackageDto>>>;

    // Handler
    internal sealed class Handler : IRequestHandler<Query, PagedResult<List<PackageDto>>>
    {
        private readonly IPackageRepository _packageRepository;

        public Handler(IPackageRepository packageRepository) => _packageRepository = packageRepository;

        public async Task<PagedResult<List<PackageDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all packages (repository does not support server-side paging)
            var allData = await _packageRepository.GetAllPackagesAsync(cancellationToken);

            int totalItems = allData.Count;
            int pageSize = request.PageSize.GetValueOrDefault(totalItems);
            int pageNumber = request.PageNumber.GetValueOrDefault(1);

            // Clamp pageSize and pageNumber to valid ranges
            if (pageSize <= 0) pageSize = totalItems;
            if (pageNumber <= 0) pageNumber = 1;

            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            int skip = (pageNumber - 1) * pageSize;

            // Efficiently get the paged data
            List<Package> pagedData;
            if (skip < totalItems && pageSize > 0)
            {
                int take = Math.Min(pageSize, totalItems - skip);
                if (allData is List<Package> list)
                {
                    pagedData = list.GetRange(skip, take);
                }
                else
                {
                    pagedData = new List<Package>(take);
                    for (int i = skip; i < skip + take; i++)
                        pagedData.Add(allData[i]);
                }
            }
            else
            {
                pagedData = allData;
            }

            // Pre-size the response list for efficiency and use factory method for clean mapping
            var response = new List<PackageDto>(pagedData.Count);
            foreach (var package in pagedData)
            {
                response.Add(PackageDto.FromEntity(package));
            }

            return new PagedResult<List<PackageDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetPackagesQueryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/packages", async (int? pageSize, int? pageNumber, ISender sender) =>
        {
            var query = new GetPackagesQuery.Query(pageSize, pageNumber);

            var result = await sender.Send(query);

            return Results.Ok(result);
        }).WithTags("Packages")
          .AddEndpointFilter<AuthenticationFilter>();
    }
}

