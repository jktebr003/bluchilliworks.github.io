using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;

namespace Api.Features.Users;

public static class UpdateUser
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? EmailAddress { get; set; }
        public string? TelephoneNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? EncryptedPassword { get; set; }
        public string? DecryptedPassword { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PackageId { get; set; }
        public IEnumerable<JobResponse>? Jobs { get; set; }
        public IEnumerable<QualificationResponse>? Qualifications { get; set; }
        public IEnumerable<CertificationResponse>? Certifications { get; set; }
        public UserType UserType { get; set; }
        public int? Avatar { get; set; } = 17;
        public DateTime ModifiedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
        public string ModifiedBy { get; set; } = "system";
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
                return new ApiResult<string>(string.Empty, false, "UpdateUser.Validation", errors);
            }

            var package = await _mongoDbRepository.Get<Package>(request.PackageId);
            if (package == null)
            {
                return new ApiResult<string>(string.Empty, false, "UpdateUser.PackageNotFound", "The specified package was not found");
            }

            var record = await _userRepository.GetUserByIdAsync(request.Id);
            if (record == null)
            {
                return new ApiResult<string>(string.Empty, false, "UpdateUser.UserNotFound", "The specified user was not found");
            }

            record.Name = request.Name;
            record.FirstName = request.FirstName;
            record.LastName = request.LastName;
            record.Username = request.Username;
            record.EmailAddress = request.EmailAddress;
            record.TelephoneNumber = request.TelephoneNumber;
            record.MobileNumber = request.MobileNumber;
            record.EncryptedPassword = request.EncryptedPassword;
            record.DecryptedPassword = request.DecryptedPassword;
            record.Gender = request.Gender;
            record.DateOfBirth = request.DateOfBirth;
            record.Avatar = request.Avatar;
            record.UserType = (int)request.UserType;
            record.Package = new Package
            {
                Name = package.Name,
                Price = package.Price,
                Code = package.Code,
                Description = package.Description,
                ID = package.ID,
                Type = (int)package.Type
            };

            // Efficient conversion for Jobs
            if (request.Jobs is { })
            {
                var jobsList = request.Jobs as ICollection<JobResponse> ?? [.. request.Jobs];
                if (jobsList.Count > 0)
                {
                    var jobs = new List<Job>(jobsList.Count);
                    foreach (var j in jobsList)
                    {
                        jobs.Add(new Job
                        {
                            Company = j.Company,
                            EndDate = j.EndDate,
                            Position = j.Position,
                            Responsibilities = j.Responsibilities,
                            StartDate = j.StartDate
                        });
                    }
                    record.Jobs = jobs;
                }
                else
                {
                    record.Jobs = null;
                }
            }
            else
            {
                record.Jobs = null;
            }

            // Efficient conversion for Qualifications
            if (request.Qualifications is { })
            {
                var qualList = request.Qualifications as ICollection<QualificationResponse> ?? [.. request.Qualifications];
                if (qualList.Count > 0)
                {
                    var quals = new List<Qualification>(qualList.Count);
                    foreach (var q in qualList)
                    {
                        quals.Add(new Qualification
                        {
                            Institution = q.Institution,
                            Title = q.Title,
                            Year = q.Year
                        });
                    }
                    record.Qualifications = quals;
                }
                else
                {
                    record.Qualifications = null;
                }
            }
            else
            {
                record.Qualifications = null;
            }

            // Efficient conversion for Certifications
            if (request.Certifications is { })
            {
                var certList = request.Certifications as ICollection<CertificationResponse> ?? [.. request.Certifications];
                if (certList.Count > 0)
                {
                    var certs = new List<Certification>(certList.Count);
                    foreach (var c in certList)
                    {
                        certs.Add(new Certification
                        {
                            Institution = c.Institution,
                            Title = c.Title,
                            Year = c.Year
                        });
                    }
                    record.Certifications = certs;
                }
                else
                {
                    record.Certifications = null;
                }
            }
            else
            {
                record.Certifications = null;
            }

            record.ModifiedBy = request.ModifiedBy;
            record.ModifiedOn = request.ModifiedOn.ToString("o");

            await _userRepository.SaveUserAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("users", async (UpdateUserRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<UpdateUser.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
