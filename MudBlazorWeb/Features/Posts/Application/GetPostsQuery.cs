using Carter;

using MediatR;

using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Posts.Application;

public static class GetPostsQuery
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

	public record Query(int? PageSize, int? PageNumber) : IRequest<PagedResult<List<PostDto>>>;

	internal sealed class Handler : IRequestHandler<Query, PagedResult<List<PostDto>>>
	{
		private readonly IPostRepository _postRepository;

		public Handler(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<PagedResult<List<PostDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			var allData = await _postRepository.GetAllPostsAsync(cancellationToken);

			int totalItems = allData.Count;
			int pageSize = request.PageSize.GetValueOrDefault(totalItems == 0 ? 1 : totalItems);
			int pageNumber = request.PageNumber.GetValueOrDefault(1);

			if (pageSize <= 0) pageSize = totalItems == 0 ? 1 : totalItems;
			if (pageNumber <= 0) pageNumber = 1;

			int totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);
			int skip = (pageNumber - 1) * pageSize;

			List<Post> pagedData;
			if (skip < totalItems)
			{
				int take = Math.Min(pageSize, totalItems - skip);
				pagedData = allData.GetRange(skip, take);
			}
			else
			{
				pagedData = [];
			}

			var response = new List<PostDto>(pagedData.Count);
			foreach (var post in pagedData)
			{
				response.Add(PostDto.FromEntity(post));
			}

			return new PagedResult<List<PostDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
		}
	}
}

public class GetPostsQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/posts", async (int? pageSize, int? pageNumber, ISender sender) =>
		{
			var query = new GetPostsQuery.Query(pageSize, pageNumber);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Posts")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
