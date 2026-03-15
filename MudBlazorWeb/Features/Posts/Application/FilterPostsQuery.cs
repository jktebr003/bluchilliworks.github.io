using Carter;

using MediatR;

using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Posts.Application;

public static class FilterPostsQuery
{
	public record PostDto(
		string Title,
		string Heading,
		string? Description,
		string? Content,
		string Author,
		string? UserId,
		string Category,
		int TotalViews,
		int TotalLikes,
		DateTime PostedOn) : BaseAuditableDto
	{
		public static PostDto FromEntity(Post post)
		{
			return new PostDto(
				post.Title,
				post.Heading,
				post.Description,
				post.Content,
				post.Author,
				post.UserId,
				post.Category,
				post.TotalViews,
				post.Likes.Count,
				post.PostedOn)
			{
				Id = post.Id,
				CreatedOn = post.CreatedOn,
				CreatedBy = post.CreatedBy,
				ModifiedOn = post.ModifiedOn,
				ModifiedBy = post.ModifiedBy,
				DeletedOn = post.DeletedOn,
				DeletedBy = post.DeletedBy,
				IsDeleted = post.IsDeleted
			};
		}
	}

	public record Query(string UserId) : IRequest<Result<List<PostDto>>>;

	internal sealed class Handler : IRequestHandler<Query, Result<List<PostDto>>>
	{
		private readonly IPostRepository _postRepository;

		public Handler(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<Result<List<PostDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			var posts = await _postRepository.FilterPostsByUserIdAsync(request.UserId, cancellationToken);
			var response = posts.Select(PostDto.FromEntity).ToList();

			return new Result<List<PostDto>>(response, true);
		}
	}
}

public class FilterPostsQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/posts/filter/{userId}", async (string userId, ISender sender) =>
		{
			var query = new FilterPostsQuery.Query(userId);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Posts")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
