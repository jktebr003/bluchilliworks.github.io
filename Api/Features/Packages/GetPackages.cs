using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Packages;

public static class GetPackages
{
    public class Query : IRequest<PagedApiResult<List<PackageResponse>>>
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, PagedApiResult<List<PackageResponse>>>
    {
        private readonly IPackageRepository _packageRepository;

        public Handler(IPackageRepository packageRepository) => _packageRepository = packageRepository;

        public async Task<PagedApiResult<List<PackageResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all packages (repository does not support server-side paging)
            var allData = await _packageRepository.GetAllPackagesAsync();

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

            // Pre-size the response list for efficiency
            var response = new List<PackageResponse>(pagedData.Count);
            foreach (var q in pagedData)
            {
                response.Add(new PackageResponse
                {
                    ID = q.ID,
                    Code = q.Code,
                    CreatedBy = q.CreatedBy,
                    CreatedOn = q.CreatedOn,
                    DeletedBy = q.DeletedBy,
                    DeletedOn = q.DeletedOn,
                    Description = q.Description,
                    IsDeleted = q.IsDeleted,
                    ModifiedBy = q.ModifiedBy,
                    ModifiedOn = q.ModifiedOn,
                    Name = q.Name,
                    Price = q.Price,
                    Type = (Shared.Enums.PackageType)q.Type
                });
            }

            return new PagedApiResult<List<PackageResponse>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetPackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("packages", async (int? pageSize, int? pageNumber, ISender sender) =>
            Results.Ok(await sender.Send(new GetPackages.Query { PageSize = pageSize, PageNumber = pageNumber }))
        )
        .WithTags("Packages")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
