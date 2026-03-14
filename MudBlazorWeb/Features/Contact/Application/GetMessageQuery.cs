using Carter;

using MediatR;

using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Contact.Application;

public static class GetMessageQuery
{
	// DTO
	public record MessageDto(
		string Name,
		string EmailAddress,
		string Subject,
		string? Body,
		DateTime? SentOn,
		MessageStatus Status,
		int AttemptCount,
		int MaxRetries,
		DateTime? LastAttemptedOn,
		string? LastErrorMessage) : BaseAuditableDto
	{
		// Factory method to create from domain entity
		public static MessageDto FromEntity(Message message)
		{
			return new MessageDto(
				message.Name,
				message.EmailAddress,
				message.Subject,
				message.Body,
				message.SentOn,
				(MessageStatus)message.Status,
				message.AttemptCount,
				message.MaxRetries,
				message.LastAttemptedOn,
				message.LastErrorMessage
			)
			{
				Id = message.Id,
				CreatedOn = message.CreatedOn,
				CreatedBy = message.CreatedBy,
				ModifiedOn = message.ModifiedOn,
				ModifiedBy = message.ModifiedBy,
				DeletedOn = message.DeletedOn,
				DeletedBy = message.DeletedBy,
				IsDeleted = message.IsDeleted
			};
		}
	}

	// Query
	public record Query(Guid Id) : IRequest<Result<MessageDto>>;

	// Handler
	internal sealed class Handler : IRequestHandler<Query, Result<MessageDto>>
	{
		private readonly IMessageRepository _messageRepository;

		public Handler(IMessageRepository messageRepository) => _messageRepository = messageRepository;

		public async Task<Result<MessageDto>> Handle(Query request, CancellationToken cancellationToken)
		{
			var data = await _messageRepository.GetMessageByIdAsync(request.Id, cancellationToken);
			if (data == null)
			{
				return new Result<MessageDto>(MessageDtoEmpty.Instance, false, "GetMessage.Null", "The message with the specified ID was not found");
			}

			// Use factory method for clean mapping
			return new Result<MessageDto>(MessageDto.FromEntity(data), true);
		}

		// Static empty response to avoid unnecessary allocations
		private sealed record MessageDtoEmpty : MessageDto
		{
			public static readonly MessageDtoEmpty Instance = new();
			private MessageDtoEmpty() : base(string.Empty, string.Empty, string.Empty, null, null, default, 0, 0, null, null) { }
		}
	}
}

public class GetMessageQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/messages/{id}", async (Guid id, ISender sender) =>
		{
			var query = new GetMessageQuery.Query(id);

			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Messages")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
