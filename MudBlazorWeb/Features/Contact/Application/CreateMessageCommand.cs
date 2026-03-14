using System;
using System.Globalization;
using Carter;
using MediatR;
using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Contact.Application;

public static class CreateMessageCommand
{
	public record Command(CreateMessageRequest Request) : IRequest<Result<string>>;

	public sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IMessageRepository _messageRepository;

		public Handler(IMessageRepository messageRepository)
		{
			_messageRepository = messageRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Request.EmailAddress) ||
				string.IsNullOrWhiteSpace(request.Request.Subject) ||
				string.IsNullOrWhiteSpace(request.Request.Body))
			{
				return new Result<string>(string.Empty, false, "CreateMessage.Validation", "Please provide email, subject, and message body.");
			}

			DateTime? sentOn = null;
			if (!string.IsNullOrWhiteSpace(request.Request.SentOn) &&
				DateTime.TryParse(
					request.Request.SentOn,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
					out var parsedSentOn))
			{
				sentOn = NormalizeToUtc(parsedSentOn);
			}

			var message = new Message
			{
				Id = Guid.NewGuid(),
				Name = request.Request.From ?? string.Empty,
				EmailAddress = request.Request.EmailAddress,
				Subject = request.Request.Subject,
				Body = request.Request.Body,
				SentOn = sentOn,
				Status = (int)MessageStatus.Pending,
				AttemptCount = 0,
				MaxRetries = 3,
				CreatedOn = DateTime.UtcNow,
				CreatedBy = request.Request.CreatedBy
			};

			await _messageRepository.SaveMessageAsync(message, cancellationToken);

			return new Result<string>(message.Id.ToString(), true);
		}

		private static DateTime NormalizeToUtc(DateTime dateTime)
		{
			return dateTime.Kind switch
			{
				DateTimeKind.Utc => dateTime,
				DateTimeKind.Local => dateTime.ToUniversalTime(),
				_ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
			};
		}
	}
}

public class CreateMessageCommandEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("api/messages", async (CreateMessageRequest request, ISender sender) =>
		{
			var command = new CreateMessageCommand.Command(request);

			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("Messages")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
