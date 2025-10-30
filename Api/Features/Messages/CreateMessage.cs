using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;

using Carter;

using FluentValidation;

using Mapster;

using MediatR;

using Shared.Models;

namespace Api.Features.Messages;

public static class CreateMessage
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? From { get; set; }
        public string? EmailAddress { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? SentOn { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Body).NotEmpty().WithMessage("Please ensure that you have entered your Message {PropertyName}");
            RuleFor(c => c.EmailAddress).NotEmpty().WithMessage("Please ensure that you have entered your Message {PropertyName}");
            RuleFor(c => c.Subject).NotEmpty().WithMessage("Please ensure that you have entered your Message {PropertyName}");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IMongoDbRepository mongoDbRepository, IMessageRepository messageRepository, IValidator<Command> validator)
        {
            // _mongoDbRepository is not used, so we can remove it for efficiency
            _messageRepository = messageRepository;
            _validator = validator;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                // Use string.Join for error messages to avoid ToString() overhead
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "CreateMessage.Validation", errors);
            }

            // Avoid unnecessary object creation by using object initializer directly
            var record = new Message
            {
                Body = request.Body,
                EmailAddress = request.EmailAddress,
                Name = request.From,
                SentOn = request.SentOn,
                Subject = request.Subject,
                CreatedOn = request.CreatedOn.ToString("o"),
                CreatedBy = request.CreatedBy
            };

            // Await directly, ConfigureAwait(false) is not needed in ASP.NET Core
            await _messageRepository.SaveMessageAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class CreateMessageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("messages", async (CreateMessageRequest request, ISender sender) =>
        {
            // Adapt is efficient, but we can avoid an extra variable
            var result = await sender.Send(request.Adapt<CreateMessage.Command>());
            return Results.Ok(result);
        })
        .WithTags("Messages")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
