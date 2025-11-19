using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class CreateUser
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? EmailAddress { get; set; }
        public string? EncryptedPassword { get; set; }
        public string? DecryptedPassword { get; set; }
        public string? PackageId { get; set; }
        public UserType UserType { get; set; }
        public int? Avatar { get; set; } = 17;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.EmailAddress).NotEmpty().WithMessage("Please ensure that you have entered your User {PropertyName}");
            RuleFor(c => c.Username).NotEmpty().WithMessage("Please ensure that you have entered your User {PropertyName}");
            RuleFor(c => c.DecryptedPassword).NotEmpty().WithMessage("Please ensure that you have entered your User {PropertyName}");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IMongoDbRepository _mongoDbRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IMongoDbRepository mongoDbRepository, IUserRepository userRepository, IValidator<Command> validator)
        {
            _mongoDbRepository = mongoDbRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                // Aggregate error messages for clarity and efficiency
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "CreateUser.Validation", errors);
            }

            // Await directly, ConfigureAwait(false) is not needed in ASP.NET Core
            var package = await _mongoDbRepository.Get<Package>(request.PackageId);
            if (package == null)
            {
                // return new ApiResult<string>(string.Empty, false, "CreateUser.PackageNotFound", "The specified package was not found");
            }

            // Use object initializer directly
            var record = new User
            {
                Avatar = request.Avatar,
                DecryptedPassword = request.DecryptedPassword,
                EmailAddress = request.EmailAddress,
                EncryptedPassword = request.EncryptedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Name = request.Name,
                // Package = new Package
                // {
                //     Name = package.Name,
                //     Price = package.Price,
                //     Code = package.Code,
                //     Description = package.Description,
                //     ID = package.ID,
                //     Type = (int)package.Type
                // },
                Username = request.Username,
                UserType = (int)request.UserType,
                CreatedOn = request.CreatedOn.ToString("o"),
                CreatedBy = request.CreatedBy
            };

            await _userRepository.SaveUserAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async (CreateUserRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<CreateUser.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
