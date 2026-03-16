using Carter;

using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.Application;

public static class CreateUserCommand
{
	public record Command(CreateUserRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IUserRepository _userRepository;

		public Handler(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Request.EmailAddress) ||
				string.IsNullOrWhiteSpace(request.Request.Username) ||
				string.IsNullOrWhiteSpace(request.Request.FirstName) ||
				string.IsNullOrWhiteSpace(request.Request.LastName))
			{
				return new Result<string>(string.Empty, false, "CreateUser.Validation", "Please provide email address, username, first name, and last name.");
			}

			var existing = await _userRepository.GetUserByEmailAddressAsync(request.Request.EmailAddress, cancellationToken);
			if (existing != null)
			{
				return new Result<string>(string.Empty, false, "CreateUser.UserExists", "A user with this email address already exists.");
			}

			Guid packageId = Guid.Empty;
			if (!string.IsNullOrWhiteSpace(request.Request.PackageId))
			{
				Guid.TryParse(request.Request.PackageId, out packageId);
			}

			var now = DateTime.UtcNow;
			var user = new User
			{
				Id = Guid.NewGuid(),
				Name = string.IsNullOrWhiteSpace(request.Request.Name)
					? $"{request.Request.FirstName} {request.Request.LastName}".Trim()
					: request.Request.Name,
				FirstName = request.Request.FirstName,
				LastName = request.Request.LastName,
				Username = request.Request.Username,
				EmailAddress = request.Request.EmailAddress,
				TelephoneNumber = null,
				MobileNumber = null,
				HashedPassword = string.Empty,
				EmailVerified = false,
				EmailVerificationToken = null,
				EmailVerificationTokenExpiry = null,
				PasswordResetToken = null,
				PasswordResetTokenExpiry = null,
				Gender = null,
				DateOfBirth = null,
				PackageId = packageId,
				Avatar = request.Request.Avatar ?? 17,
				UserType = request.Request.UserType ?? 0,
				Skills = null,
				Hobbies = null,
				CreatedOn = request.Request.CreatedOn == default ? now : request.Request.CreatedOn,
				CreatedBy = request.Request.CreatedBy
			};

			await _userRepository.SaveUserAsync(user, cancellationToken);

			return new Result<string>(user.Id.ToString(), true);
		}
	}
}

public static class UpdateUserCommand
{
	public record Command(UpdateUserRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IUserRepository _userRepository;

		public Handler(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			// if (string.IsNullOrWhiteSpace(request.Request.Id) || !Guid.TryParse(request.Request.Id, out _))
			// {
			// 	return new Result<string>(string.Empty, false, "UpdateUser.Validation", "Invalid user ID.");
			// }

			if (string.IsNullOrWhiteSpace(request.Request.EmailAddress) ||
				string.IsNullOrWhiteSpace(request.Request.Username))
			{
				return new Result<string>(string.Empty, false, "UpdateUser.Validation", "Please provide email address and username.");
			}

			User existingUser;
			try
			{
				existingUser = await _userRepository.GetUserByIdAsync(request.Request.Id, cancellationToken);
			}
			catch (InvalidOperationException)
			{
				return new Result<string>(string.Empty, false, "UpdateUser.NotFound", "The user with the specified ID was not found.");
			}

			if (!string.IsNullOrWhiteSpace(request.Request.PackageId) &&
				Guid.TryParse(request.Request.PackageId, out var parsedPackageId))
			{
				existingUser.PackageId = parsedPackageId;
			}

			existingUser.Name = request.Request.Name ?? existingUser.Name;
			existingUser.FirstName = request.Request.FirstName ?? existingUser.FirstName;
			existingUser.LastName = request.Request.LastName ?? existingUser.LastName;
			existingUser.Username = request.Request.Username;
			existingUser.EmailAddress = request.Request.EmailAddress;
			existingUser.TelephoneNumber = request.Request.TelephoneNumber;
			existingUser.MobileNumber = request.Request.MobileNumber;
			existingUser.Gender = request.Request.Gender;
			existingUser.DateOfBirth = request.Request.DateOfBirth;
			existingUser.Avatar = request.Request.Avatar ?? existingUser.Avatar;
			existingUser.UserType = (int)request.Request.UserType;
			existingUser.Skills = request.Request.Skills;
			existingUser.Hobbies = request.Request.Hobbies;
			existingUser.ModifiedOn = request.Request.ModifiedOn;
			existingUser.ModifiedBy = request.Request.ModifiedBy;

			if (request.Request.Jobs != null)
			{
				existingUser.Jobs = request.Request.Jobs.Select(j => new Job
				{
					Id = Guid.NewGuid(),
					UserId = existingUser.Id,
					Company = j.Company,
					Position = j.Position,
					StartDate = j.StartDate,
					EndDate = j.EndDate,
					Responsibilities = j.Responsibilities
				}).ToList();
			}

			if (request.Request.Qualifications != null)
			{
				existingUser.Qualifications = request.Request.Qualifications.Select(q => new Qualification
				{
					Id = Guid.NewGuid(),
					UserId = existingUser.Id,
					Title = q.Title,
					Institution = q.Institution,
					Year = q.Year
				}).ToList();
			}

			if (request.Request.Certifications != null)
			{
				existingUser.Certifications = request.Request.Certifications.Select(c => new Certification
				{
					Id = Guid.NewGuid(),
					UserId = existingUser.Id,
					Title = c.Title,
					Institution = c.Institution,
					Year = c.Year
				}).ToList();
			}

			await _userRepository.UpdateUserAsync(existingUser, cancellationToken);

			return new Result<string>(existingUser.Id.ToString(), true);
		}
	}
}

public class UserCommandsEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("api/users", async (CreateUserRequest request, ISender sender) =>
		{
			var command = new CreateUserCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("Users")
		  .AddEndpointFilter<AuthenticationFilter>();

		app.MapPut("api/users", async (UpdateUserRequest request, ISender sender) =>
		{
			var command = new UpdateUserCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("Users")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
