using Api.Filters;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Posts;

public static class GetPost
{
    public class Query : IRequest<ApiResult<PostResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<PostResponse>>
    {
        private readonly IPostRepository _postRepository;

        public Handler(IPostRepository postRepository) => _postRepository = postRepository;

        public async Task<ApiResult<PostResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Remove ConfigureAwait(false) for ASP.NET Core context consistency
            var data = await _postRepository.GetPostByIdAsync(request.Id);
            if (data == null)
            {
                // Use static empty response to avoid repeated allocations
                return new ApiResult<PostResponse>(PostResponseEmpty.Instance, false, "GetPost.Null", "The post with the specified ID was not found");
            }

            // Use object initializer directly in return for clarity and efficiency
            return new ApiResult<PostResponse>(
                new PostResponse
                {
                    ID = data.ID,
                    Author = data.Author,
                    Category = data.Category,
                    Content = data.Content,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    DeletedBy = data.DeletedBy,
                    DeletedOn = data.DeletedOn,
                    Description = data.Description,
                    Heading = data.Heading,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedOn = data.ModifiedOn,
                    PostedOn = data.PostedOn,
                    Title = data.Title,
                    TotalComments = data.Comments?.Count,
                    TotalLikes = data.Likes?.Count,
                    TotalViews = data.TotalViews,
                    UserId = data.UserId
                },
                true
            );
        }

        // Static empty response to avoid unnecessary allocations
        private sealed class PostResponseEmpty : PostResponse
        {
            public static readonly PostResponseEmpty Instance = new();
            private PostResponseEmpty() { }
        }
    }
}

public class GetPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/{id}", async (string id, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new GetPost.Query { Id = id });
            return Results.Ok(result);
        })
        .WithTags("Posts")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
