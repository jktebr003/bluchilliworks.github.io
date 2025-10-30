using Api.Filters;
using Carter;
using MediatR;
using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class GetUser
{
    public class Query : IRequest<ApiResult<UserResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<UserResponse>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<ApiResult<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Remove ConfigureAwait(false) for ASP.NET Core context consistency
            var data = await _userRepository.GetUserByIdAsync(request.Id);
            if (data == null)
            {
                // Use static empty response to avoid repeated allocations
                return new ApiResult<UserResponse>(UserResponseEmpty.Instance, false, "GetUser.Null", "The user with the specified ID was not found");
            }

            // Use efficient list conversion and object initializers
            var response = new UserResponse
            {
                ID = data.ID,
                Name = data.Name,
                Certifications = data.Certifications is { Count: > 0 }
                    ? data.Certifications.ConvertAll(r => new CertificationResponse
                    {
                        Institution = r.Institution,
                        Title = r.Title,
                        Year = r.Year
                    })
                    : null,
                CreatedBy = data.CreatedBy,
                CreatedOn = data.CreatedOn,
                DateOfBirth = data.DateOfBirth,
                DecryptedPassword = data.DecryptedPassword,
                EmailAddress = data.EmailAddress,
                EncryptedPassword = data.EncryptedPassword,
                FirstName = data.FirstName,
                Gender = data.Gender,
                Jobs = data.Jobs is { Count: > 0 }
                    ? data.Jobs.ConvertAll(r => new JobResponse
                    {
                        Company = r.Company,
                        EndDate = r.EndDate,
                        Position = r.Position,
                        Responsibilities = r.Responsibilities,
                        StartDate = r.StartDate
                    })
                    : null,
                LastName = data.LastName,
                MobileNumber = data.MobileNumber,
                ModifiedBy = data.ModifiedBy,
                ModifiedOn = data.ModifiedOn,
                Package = data.Package is not null ? new PackageResponse
                {
                    ID = data.Package.ID,
                    Name = data.Package.Name,
                    Price = data.Package.Price,
                    Code = data.Package.Code,
                    Description = data.Package.Description,
                    Type = (PackageType)data.Package.Type
                } : null,
                Qualifications = data.Qualifications is { Count: > 0 }
                    ? data.Qualifications.ConvertAll(r => new QualificationResponse
                    {
                        Institution = r.Institution,
                        Title = r.Title,
                        Year = r.Year
                    })
                    : null,
                TelephoneNumber = data.TelephoneNumber,
                Username = data.Username,
                UserRole = data.UserType is not null ? (UserType)data.UserType : UserType.None
            };

            return new ApiResult<UserResponse>(response, true);
        }

        // Static empty response to avoid unnecessary allocations
        private sealed class UserResponseEmpty : UserResponse
        {
            public static readonly UserResponseEmpty Instance = new();
            private UserResponseEmpty() { }
        }
    }
}

public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (string id, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new GetUser.Query { Id = id });
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
