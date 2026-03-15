using Carter;

using MediatR;

using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Posts.Application;

public static class GetPostQuery
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

	public record Query(Guid Id) : IRequest<Result<PostDto>>;

	internal sealed class Handler : IRequestHandler<Query, Result<PostDto>>
	{
		private readonly IPostRepository _postRepository;

		public Handler(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<Result<PostDto>> Handle(Query request, CancellationToken cancellationToken)
		{
			var data = await _postRepository.GetPostByIdAsync(request.Id, cancellationToken);
			if (data == null)
			{
				return new Result<PostDto>(PostDtoEmpty.Instance, false, "GetPost.Null", "The post with the specified ID was not found");
			}

			return new Result<PostDto>(PostDto.FromEntity(data), true);
		}

		private sealed record PostDtoEmpty : PostDto
		{
			public static readonly PostDtoEmpty Instance = new();
			private PostDtoEmpty() : base(string.Empty, string.Empty, null, null, string.Empty, null, string.Empty, 0, 0, DateTime.MinValue) { }
		}
	}
}

public class GetPostQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/posts/{id}", async (Guid id, ISender sender) =>
		{
			var query = new GetPostQuery.Query(id);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Posts")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
