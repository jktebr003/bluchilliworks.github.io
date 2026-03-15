using Carter;

using MediatR;

using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Posts.Application;

public static class CreatePostCommand
{
	public record Command(CreatePostRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IPostRepository _postRepository;

		public Handler(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Request.Title) ||
				string.IsNullOrWhiteSpace(request.Request.Heading) ||
				string.IsNullOrWhiteSpace(request.Request.Content) ||
				string.IsNullOrWhiteSpace(request.Request.Author) ||
				string.IsNullOrWhiteSpace(request.Request.Category))
			{
				return new Result<string>(string.Empty, false, "CreatePost.Validation", "Please provide title, heading, content, author, and category.");
			}

			var post = new Post
			{
				Id = Guid.NewGuid(),
				Title = request.Request.Title,
				Heading = request.Request.Heading,
				Description = request.Request.Description ?? string.Empty,
				Content = request.Request.Content,
				Author = request.Request.Author,
				UserId = request.Request.UserId ?? string.Empty,
				Category = request.Request.Category,
				TotalViews = 0,
				Likes = [],
				PostedOn = DateTime.UtcNow,
				CreatedOn = DateTime.UtcNow,
				CreatedBy = request.Request.CreatedBy
			};

			await _postRepository.SavePostAsync(post, cancellationToken);

			return new Result<string>(post.Id.ToString(), true);
		}
	}
}

public static class UpdatePostCommand
{
	public record Command(UpdatePostRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IPostRepository _postRepository;

		public Handler(IPostRepository postRepository)
		{
			_postRepository = postRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (!Guid.TryParse(request.Request.Id, out var postId))
			{
				return new Result<string>(string.Empty, false, "UpdatePost.Validation", "Invalid post ID.");
			}

			var existingPost = await _postRepository.GetPostByIdAsync(postId, cancellationToken);
			if (existingPost == null)
			{
				return new Result<string>(string.Empty, false, "UpdatePost.NotFound", "The post with the specified ID was not found.");
			}

			existingPost.Title = request.Request.Title ?? existingPost.Title;
			existingPost.Heading = request.Request.Heading ?? existingPost.Heading;
			existingPost.Description = request.Request.Description ?? existingPost.Description;
			existingPost.Content = request.Request.Content ?? existingPost.Content;
			existingPost.Author = request.Request.Author ?? existingPost.Author;
			existingPost.UserId = request.Request.UserId ?? existingPost.UserId;
			existingPost.Category = request.Request.Category ?? existingPost.Category;
			existingPost.TotalViews = request.Request.TotalViews ?? existingPost.TotalViews;
			existingPost.Likes = request.Request.Likes?.Count > 0 ? request.Request.Likes : existingPost.Likes;
			existingPost.ModifiedOn = request.Request.ModifiedOn;
			existingPost.ModifiedBy = request.Request.ModifiedBy;

			await _postRepository.UpdatePostAsync(existingPost, cancellationToken);

			return new Result<string>(existingPost.Id.ToString(), true);
		}
	}
}

public class CreatePostCommandEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("api/posts", async (CreatePostRequest request, ISender sender) =>
		{
			var command = new CreatePostCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("Posts")
		  .AddEndpointFilter<AuthenticationFilter>();

		app.MapPut("api/posts", async (UpdatePostRequest request, ISender sender) =>
		{
			var command = new UpdatePostCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("Posts")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
