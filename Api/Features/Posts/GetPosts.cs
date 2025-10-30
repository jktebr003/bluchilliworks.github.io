using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Posts;

public static class GetPosts
{
    public class Query : IRequest<PagedApiResult<List<PostResponse>>>
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, PagedApiResult<List<PostResponse>>>
    {
        private readonly IPostRepository _postRepository;

        public Handler(IPostRepository postRepository) => _postRepository = postRepository;

        public async Task<PagedApiResult<List<PostResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all posts (repository does not support server-side paging)
            var allData = await _postRepository.GetAllPostsAsync();

            int totalItems = allData.Count;
            int pageSize = request.PageSize.GetValueOrDefault(totalItems);
            int pageNumber = request.PageNumber.GetValueOrDefault(1);

            // Clamp pageSize and pageNumber to valid ranges
            if (pageSize <= 0) pageSize = totalItems;
            if (pageNumber <= 0) pageNumber = 1;

            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            int skip = (pageNumber - 1) * pageSize;

            // Efficiently get the paged data
            List<Post> pagedData;
            if (skip < totalItems && pageSize > 0)
            {
                int take = Math.Min(pageSize, totalItems - skip);
                if (allData is List<Post> list)
                {
                    pagedData = list.GetRange(skip, take);
                }
                else
                {
                    pagedData = new List<Post>(take);
                    for (int i = skip; i < skip + take; i++)
                        pagedData.Add(allData[i]);
                }
            }
            else
            {
                pagedData = allData;
            }

            // Pre-size the response list for efficiency
            var response = new List<PostResponse>(pagedData.Count);
            foreach (var q in pagedData)
            {
                response.Add(new PostResponse
                {
                    ID = q.ID,
                    Author = q.Author,
                    Category = q.Category,
                    Content = q.Content,
                    CreatedBy = q.CreatedBy,
                    CreatedOn = q.CreatedOn,
                    DeletedBy = q.DeletedBy,
                    DeletedOn = q.DeletedOn,
                    Description = q.Description,
                    Heading = q.Heading,
                    IsDeleted = q.IsDeleted,
                    ModifiedBy = q.ModifiedBy,
                    ModifiedOn = q.ModifiedOn,
                    PostedOn = q.PostedOn,
                    Title = q.Title,
                    TotalComments = q.Comments?.Count,
                    TotalLikes = q.Likes?.Count,
                    TotalViews = q.TotalViews,
                    UserId = q.UserId
                });
            }

            return new PagedApiResult<List<PostResponse>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("posts", async (int? pageSize, int? pageNumber, ISender sender) =>
            Results.Ok(await sender.Send(new GetPosts.Query { PageSize = pageSize, PageNumber = pageNumber }))
        )
        .WithTags("Posts")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
