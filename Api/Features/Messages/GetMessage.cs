using Api.Filters;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Messages;

public static class GetMessage
{
    public class Query : IRequest<ApiResult<MessageResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<MessageResponse>>
    {
        private readonly IMessageRepository _messageRepository;

        public Handler(IMessageRepository messageRepository) => _messageRepository = messageRepository;

        public async Task<ApiResult<MessageResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Remove ConfigureAwait(false) for ASP.NET Core context consistency
            var data = await _messageRepository.GetMessageByIdAsync(request.Id);
            if (data == null)
            {
                // Use static instance for empty response to avoid unnecessary allocations
                return new ApiResult<MessageResponse>(MessageResponseEmpty.Instance, false, "GetMessage.Null", "The message with the specified ID was not found");
            }

            // Use object initializer and avoid unnecessary variable
            return new ApiResult<MessageResponse>(
                new MessageResponse
                {
                    ID = data.ID,
                    Body = data.Body,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    DeletedBy = data.DeletedBy,
                    DeletedOn = data.DeletedOn,
                    EmailAddress = data.EmailAddress,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedOn = data.ModifiedOn,
                    From = data.Name,
                    SentOn = data.SentOn,
                    Subject = data.Subject
                },
                true
            );
        }

        // Static empty response to avoid repeated allocations
        private sealed class MessageResponseEmpty : MessageResponse
        {
            public static readonly MessageResponseEmpty Instance = new();
            private MessageResponseEmpty() { }
        }
    }
}

public class GetMessageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("messages/{id}", async (string id, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new GetMessage.Query { Id = id });
            return Results.Ok(result);
        })
        .WithTags("Messages")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
