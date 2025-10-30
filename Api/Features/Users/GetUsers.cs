using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;

using Carter;

using MediatR;

using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class GetUsers
{
    public class Query : IRequest<PagedApiResult<List<UserResponse>>>
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, PagedApiResult<List<UserResponse>>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<PagedApiResult<List<UserResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all users (repository does not support server-side paging)
            var allData = await _userRepository.GetAllUsersAsync();

            int totalItems = allData.Count;
            int pageSize = request.PageSize.GetValueOrDefault(totalItems);
            int pageNumber = request.PageNumber.GetValueOrDefault(1);

            // Clamp pageSize and pageNumber to valid ranges
            if (pageSize <= 0) pageSize = totalItems;
            if (pageNumber <= 0) pageNumber = 1;

            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            int skip = (pageNumber - 1) * pageSize;

            // Efficiently get the paged data
            List<User> pagedData;
            if (skip < totalItems && pageSize > 0)
            {
                int take = Math.Min(pageSize, totalItems - skip);
                if (allData is List<User> list)
                {
                    pagedData = list.GetRange(skip, take);
                }
                else
                {
                    pagedData = new List<User>(take);
                    for (int i = skip; i < skip + take; i++)
                        pagedData.Add(allData[i]);
                }
            }
            else
            {
                pagedData = allData;
            }

            // Pre-size the response list for efficiency
            var response = new List<UserResponse>(pagedData.Count);
            foreach (var q in pagedData)
            {
                response.Add(new UserResponse
                {
                    ID = q.ID,
                    Name = q.Name,
                    Certifications = q.Certifications is { Count: > 0 }
                        ? q.Certifications.ConvertAll(r => new CertificationResponse
                        {
                            Institution = r.Institution,
                            Title = r.Title,
                            Year = r.Year
                        })
                        : null,
                    CreatedBy = q.CreatedBy,
                    CreatedOn = q.CreatedOn,
                    DateOfBirth = q.DateOfBirth,
                    DecryptedPassword = q.DecryptedPassword,
                    EmailAddress = q.EmailAddress,
                    EncryptedPassword = q.EncryptedPassword,
                    FirstName = q.FirstName,
                    Gender = q.Gender,
                    Jobs = q.Jobs is { Count: > 0 }
                        ? q.Jobs.ConvertAll(r => new JobResponse
                        {
                            Company = r.Company,
                            EndDate = r.EndDate,
                            Position = r.Position,
                            Responsibilities = r.Responsibilities,
                            StartDate = r.StartDate
                        })
                        : null,
                    LastName = q.LastName,
                    MobileNumber = q.MobileNumber,
                    ModifiedBy = q.ModifiedBy,
                    ModifiedOn = q.ModifiedOn,
                    Package = q.Package is not null ? new PackageResponse
                    {
                        ID = q.Package.ID,
                        Name = q.Package.Name,
                        Price = q.Package.Price,
                        Code = q.Package.Code,
                        Description = q.Package.Description,
                        Type = (PackageType)q.Package.Type
                    } : null,
                    Qualifications = q.Qualifications is { Count: > 0 }
                        ? q.Qualifications.ConvertAll(r => new QualificationResponse
                        {
                            Institution = r.Institution,
                            Title = r.Title,
                            Year = r.Year
                        })
                        : null,
                    TelephoneNumber = q.TelephoneNumber,
                    Username = q.Username,
                    UserRole = q.UserType is not null ? (UserType)q.UserType : UserType.None
                });
            }

            return new PagedApiResult<List<UserResponse>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (int? pageSize, int? pageNumber, ISender sender) =>
            Results.Ok(await sender.Send(new GetUsers.Query { PageSize = pageSize, PageNumber = pageNumber }))
        )
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}