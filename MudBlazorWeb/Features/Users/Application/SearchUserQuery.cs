using Carter;

using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Users.Application;

public static class SearchUserQuery
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

	public record Query(
		string? Search,
		UserType? Role,
		string? Gender,
		string? Package,
		bool? EmailVerified,
		DateTime? DobFrom,
		DateTime? DobTo,
		int? PageSize,
		int? PageNumber) : IRequest<PagedResult<List<UserDto>>>;

	internal sealed class Handler : IRequestHandler<Query, PagedResult<List<UserDto>>>
	{
		private readonly IUserRepository _userRepository;

		public Handler(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<PagedResult<List<UserDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			var filteredData = await _userRepository.SearchUsersAsync(
				search: request.Search,
				role: request.Role,
				gender: request.Gender,
				package: request.Package,
				emailVerified: request.EmailVerified,
				dobFrom: request.DobFrom,
				dobTo: request.DobTo,
				cancellationToken: cancellationToken);

			int totalItems = filteredData.Count;
			int pageSize = request.PageSize.GetValueOrDefault(totalItems == 0 ? 1 : totalItems);
			int pageNumber = request.PageNumber.GetValueOrDefault(1);

			if (pageSize <= 0) pageSize = totalItems == 0 ? 1 : totalItems;
			if (pageNumber <= 0) pageNumber = 1;

			int totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);
			int skip = (pageNumber - 1) * pageSize;

			List<User> pagedData;
			if (skip < totalItems)
			{
				int take = Math.Min(pageSize, totalItems - skip);
				pagedData = filteredData.GetRange(skip, take);
			}
			else
			{
				pagedData = [];
			}

			var response = pagedData.Select(UserDto.FromEntity).ToList();

			return new PagedResult<List<UserDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
		}
	}
}

public class SearchUserQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/users/search", async (
			string? search,
			UserType? role,
			string? gender,
			string? package,
			bool? emailVerified,
			DateTime? dobFrom,
			DateTime? dobTo,
			int? pageSize,
			int? pageNumber,
			ISender sender) =>
		{
			var query = new SearchUserQuery.Query(
				search,
				role,
				gender,
				package,
				emailVerified,
				dobFrom,
				dobTo,
				pageSize,
				pageNumber);

			var result = await sender.Send(query);
			return Results.Ok(result);
		}).WithTags("Users")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
