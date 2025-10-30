using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Models;

namespace Api.Features.Posts;

public static class CreatePost
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? Title { get; set; }
        public string? Heading { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Author { get; set; }
        public string? UserId { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
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
                return new ApiResult<string>(string.Empty, false, "CreatePost.Validation", errors);
            }

            // Use object initializer directly
            var record = new Post
            {
                Author = request.Author,
                Category = request.Category,
                Content = request.Content,
                Description = request.Description,
                Heading = request.Heading,
                Title = request.Title,
                UserId = request.UserId,
                CreatedOn = request.CreatedOn.ToString("o"),
                CreatedBy = request.CreatedBy
            };

            // Await directly, ConfigureAwait(false) is not needed in ASP.NET Core
            await _postRepository.SavePostAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class CreatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("posts", async (CreatePostRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<CreatePost.Command>());
            return Results.Ok(result);
        })
        .WithTags("Posts")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
