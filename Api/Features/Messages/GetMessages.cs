using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.Messages;

public static class GetMessages
{
    public class Query : IRequest<PagedApiResult<List<MessageResponse>>>
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, PagedApiResult<List<MessageResponse>>>
    {
        private readonly IMessageRepository _messageRepository;

        public Handler(IMessageRepository messageRepository) => _messageRepository = messageRepository;

        public async Task<PagedApiResult<List<MessageResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all messages (repository does not support server-side paging)
            var allData = await _messageRepository.GetAllMessagesAsync();

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
                    // Avoids LINQ allocation if already a List
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

            // Pre-size the response list for efficiency
            var response = new List<MessageResponse>(pagedData.Count);
            foreach (var q in pagedData)
            {
                response.Add(new MessageResponse
                {
                    ID = q.ID,
                    Body = q.Body,
                    CreatedBy = q.CreatedBy,
                    CreatedOn = q.CreatedOn,
                    DeletedBy = q.DeletedBy,
                    DeletedOn = q.DeletedOn,
                    EmailAddress = q.EmailAddress,
                    IsDeleted = q.IsDeleted,
                    ModifiedBy = q.ModifiedBy,
                    ModifiedOn = q.ModifiedOn,
                    From = q.Name,
                    SentOn = q.SentOn,
                    Subject = q.Subject
                });
            }

            return new PagedApiResult<List<MessageResponse>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetMessagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("messages", async (int? pageSize, int? pageNumber, ISender sender) =>
            Results.Ok(await sender.Send(new GetMessages.Query { PageSize = pageSize, PageNumber = pageNumber }))
        )
        .WithTags("Messages")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
