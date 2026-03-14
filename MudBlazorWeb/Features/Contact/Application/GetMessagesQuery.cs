using Carter;

using MediatR;

using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Contact.Application;

public static class GetMessagesQuery
{
	// DTO (reuse from GetMessageQuery)
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
	public record Query(int? PageSize, int? PageNumber) : IRequest<PagedResult<List<MessageDto>>>;

	// Handler
	internal sealed class Handler : IRequestHandler<Query, PagedResult<List<MessageDto>>>
	{
		private readonly IMessageRepository _messageRepository;

		public Handler(IMessageRepository messageRepository) => _messageRepository = messageRepository;

		public async Task<PagedResult<List<MessageDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			// Fetch all messages (repository does not support server-side paging)
			var allData = await _messageRepository.GetAllMessagesAsync(cancellationToken);

			int totalItems = allData.Count;
			int pageSize = request.PageSize.GetValueOrDefault(totalItems);
			int pageNumber = request.PageNumber.GetValueOrDefault(1);

			// Clamp pageSize and pageNumber to valid ranges
			if (pageSize <= 0) pageSize = totalItems;
			if (pageNumber <= 0) pageNumber = 1;

			int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
			int skip = (pageNumber - 1) * pageSize;

			// Efficiently get the paged data
			List<Message> pagedData;
			if (skip < totalItems && pageSize > 0)
			{
				int take = Math.Min(pageSize, totalItems - skip);
				if (allData is List<Message> list)
				{
					pagedData = list.GetRange(skip, take);
				}
				else
				{
					pagedData = new List<Message>(take);
					for (int i = skip; i < skip + take; i++)
						pagedData.Add(allData[i]);
				}
			}
			else
			{
				pagedData = allData;
			}

			// Pre-size the response list for efficiency and use factory method for clean mapping
			var response = new List<MessageDto>(pagedData.Count);
			foreach (var message in pagedData)
			{
				response.Add(MessageDto.FromEntity(message));
			}

			return new PagedResult<List<MessageDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
		}
	}
}

public class GetMessagesQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/messages", async (int? pageSize, int? pageNumber, ISender sender) =>
		{
			var query = new GetMessagesQuery.Query(pageSize, pageNumber);

			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("Messages")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
