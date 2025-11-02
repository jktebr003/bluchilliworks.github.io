using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;

using Carter;

using MediatR;

using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class FilterUsers
{
    public class Query : IRequest<ApiResult<List<UserResponse>>>
    {
        public string EmailAddress { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<List<UserResponse>>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<ApiResult<List<UserResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch filtered users
            var filteredData = await _userRepository.FilterUsersByEmailAddressAsync(request.EmailAddress);
            
            // Pre-size the response list for efficiency
            var response = new List<UserResponse>(filteredData.Count);
            foreach (var q in filteredData)
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
                    Jobs = q.Jobs is { Count:  > 0 }
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

            return new ApiResult<List<UserResponse>>(response, true);
        }
    }
}

public class FilterUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/emailAddress/{emailAddress}", async (string emailAddress, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new FilterUsers.Query { EmailAddress = emailAddress });
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
