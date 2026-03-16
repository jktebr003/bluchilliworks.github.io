using Carter;

using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Users.Application;

public static class GetUserByIdQuery
{
	public record UserDto(
		string Name,
		string FirstName,
		string LastName,
		string Username,
		string EmailAddress,
		string? TelephoneNumber,
		string? MobileNumber,
		bool EmailVerified,
		string? Gender,
		string? DateOfBirth,
		Guid PackageId,
		int Avatar,
		UserType UserRole,
		string? Skills,
		string? Hobbies) : BaseAuditableDto
	{
		public static UserDto FromEntity(User user)
		{
			return new UserDto(
				user.Name,
				user.FirstName,
				user.LastName,
				user.Username,
				user.EmailAddress,
				user.TelephoneNumber,
				user.MobileNumber,
				user.EmailVerified,
				user.Gender,
				user.DateOfBirth,
				user.PackageId,
				user.Avatar,
				(UserType)user.UserType,
				user.Skills,
				user.Hobbies)
			{
				Id = user.Id,
				CreatedOn = user.CreatedOn,
				CreatedBy = user.CreatedBy,
				ModifiedOn = user.ModifiedOn,
				ModifiedBy = user.ModifiedBy,
				DeletedOn = user.DeletedOn,
				DeletedBy = user.DeletedBy,
				IsDeleted = user.IsDeleted
			};
		}
	}

	public record Query(Guid Id) : IRequest<Result<UserDto>>;

	internal sealed class Handler : IRequestHandler<Query, Result<UserDto>>
	{
		private readonly IUserRepository _userRepository;

		public Handler(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<Result<UserDto>> Handle(Query request, CancellationToken cancellationToken)
		{
			try
			{
				var data = await _userRepository.GetUserByIdAsync(request.Id, cancellationToken);
				return new Result<UserDto>(UserDto.FromEntity(data), true);
			}
			catch (InvalidOperationException)
			{
				return new Result<UserDto>(UserDtoEmpty.Instance, false, "GetUserById.NotFound", "The user with the specified ID was not found");
			}
		}

		private sealed record UserDtoEmpty : UserDto
		{
			public static readonly UserDtoEmpty Instance = new();
			private UserDtoEmpty() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, false, null, null, Guid.Empty, 0, UserType.None, null, null) { }
		}
	}
}

public class GetUserByIdQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/users/{id}", async (Guid id, ISender sender) =>
		{
			var query = new GetUserByIdQuery.Query(id);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Users")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
