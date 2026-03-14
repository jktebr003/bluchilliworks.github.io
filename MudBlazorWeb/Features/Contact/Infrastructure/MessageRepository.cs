using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Contact.Infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetAllMessagesAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""Name"",
                ""EmailAddress"",
                ""Subject"",
                ""Body"",
                ""SentOn"",
                ""Status"",
                ""AttemptCount"",
                ""MaxRetries"",
                ""LastAttemptedOn"",
                ""LastErrorMessage"",
                ""CreatedOn"",
                ""CreatedBy"",
                ""ModifiedOn"",
                ""ModifiedBy"",
                ""DeletedOn"",
                ""DeletedBy"",
                ""IsDeleted""
            FROM ""Messages""
            WHERE ""IsDeleted"" = false
            ORDER BY ""CreatedOn"" DESC";

        var messages = await connection.QueryAsync<Message>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return messages.ToList();
    }

    public async Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""Name"",
                ""EmailAddress"",
                ""Subject"",
                ""Body"",
                ""SentOn"",
                ""Status"",
                ""AttemptCount"",
                ""MaxRetries"",
                ""LastAttemptedOn"",
                ""LastErrorMessage"",
                ""CreatedOn"",
                ""CreatedBy"",
                ""ModifiedOn"",
                ""ModifiedBy"",
                ""DeletedOn"",
                ""DeletedBy"",
                ""IsDeleted""
            FROM ""Messages""
            WHERE ""Id"" = @Id AND ""IsDeleted"" = false";

        var message = await connection.QuerySingleOrDefaultAsync<Message>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return message;
    }

    public async Task SaveMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(message, cancellationToken);
        _context.SaveChanges(message.CreatedBy, cancellationToken);
    }
}
