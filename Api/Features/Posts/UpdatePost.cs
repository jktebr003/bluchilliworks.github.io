using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Models;

namespace Api.Features.Posts;

public static class UpdatePost
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Heading { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Author { get; set; }
        public string? UserId { get; set; }
        public string? Category { get; set; }
        public int? TotalViews { get; set; }
        public List<string> Likes { get; set; } = [];
        public List<CommentResponse> Comments { get; set; } = [];
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; } = "system";
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Author).NotEmpty().WithMessage("Please ensure that you have entered your Post {PropertyName}");
            RuleFor(c => c.Category).NotEmpty().WithMessage("Please ensure that you have entered your Post {PropertyName}");
            RuleFor(c => c.Content).NotEmpty().WithMessage("Please ensure that you have entered your Post {PropertyName}");
            RuleFor(c => c.Heading).NotEmpty().WithMessage("Please ensure that you have entered your Post {PropertyName}");
            RuleFor(c => c.Title).NotEmpty().WithMessage("Please ensure that you have entered your Post {PropertyName}");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IMongoDbRepository mongoDbRepository, IPostRepository postRepository, IValidator<Command> validator)
        {
            // _mongoDbRepository is not used, so we can remove it for efficiency
            _postRepository = postRepository;
            _validator = validator;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                // Aggregate error messages for clarity and efficiency
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "UpdatePost.Validation", errors);
            }

            var record = await _postRepository.GetPostByIdAsync(request.Id);
            if (record == null)
            {
                return new ApiResult<string>(string.Empty, false, "UpdatePost.PostNotFound", "The specified post was not found");
            }

            // Use efficient list conversion and object initialization
            record.Author = request.Author;
            record.Category = request.Category;
            if (request.Comments != null && request.Comments.Count > 0)
            {
                var comments = new List<Comment>(request.Comments.Count);
                foreach (var r in request.Comments)
                {
                    comments.Add(new Comment
                    {
                        ID = r.ID,
                        Author = r.Author,
                        CommentContent = r.Content,
                        DateCommented = r.CommentedOn,
                        CreatedBy = r.CreatedBy,
                        CreatedOn = r.CreatedOn,
                        DeletedBy = r.DeletedBy,
                        DeletedOn = r.DeletedOn,
                        IsDeleted = r.IsDeleted,
                        ModifiedBy = r.ModifiedBy,
                        ModifiedOn = r.ModifiedOn,
                        PostId = r.PostId
                    });
                }
                record.Comments = comments;
            }
            else
            {
                record.Comments = [];
            }
            record.Content = request.Content;
            record.Description = request.Description;
            record.Heading = request.Heading;
            record.Likes = request.Likes ?? [];
            record.Title = request.Title;
            record.TotalViews = request.TotalViews;
            record.UserId = request.UserId;
            record.ModifiedBy = request.ModifiedBy;
            record.ModifiedOn = request.ModifiedOn.ToString("o");

            await _postRepository.SavePostAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class UpdatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("posts", async (UpdatePostRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<UpdatePost.Command>());
            return Results.Ok(result);
        })
        .WithTags("Posts")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
