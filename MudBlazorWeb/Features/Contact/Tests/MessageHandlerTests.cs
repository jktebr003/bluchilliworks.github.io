using MudBlazorWeb.Features.Contact.Application;
using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Contact.Tests;

public class MessageHandlerTests
{
    // ======================================================================
    // CreateMessageCommand.Handler
    // ======================================================================

    [Fact]
    public async Task CreateMessageHandler_ShouldReturnSuccess_WhenAllRequiredFieldsAreProvided()
    {
        Message? savedMessage = null;

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (msg, _) =>
            {
                savedMessage = msg;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            From = "John Doe",
            EmailAddress = "john@example.com",
            Subject = "Hello World",
            Body = "This is a test message",
            SentOn = "2026-01-15T10:00:00Z",
            CreatedBy = "contact-form"
        };

        var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Code);
        Assert.NotNull(savedMessage);
        Assert.Equal("john@example.com", savedMessage!.EmailAddress);
        Assert.Equal("Hello World", savedMessage.Subject);
        Assert.Equal("This is a test message", savedMessage.Body);
        Assert.Equal("John Doe", savedMessage.Name);
        Assert.NotEqual(Guid.Empty, savedMessage.Id);
        Assert.Equal(result.Value, savedMessage.Id.ToString());
        Assert.Equal((int)MessageStatus.Pending, savedMessage.Status);
        Assert.Equal(0, savedMessage.AttemptCount);
        Assert.Equal(3, savedMessage.MaxRetries);
        Assert.Equal("contact-form", savedMessage.CreatedBy);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldReturnValidationFailure_WhenEmailAddressIsEmpty()
    {
        var repository = new FakeMessageRepository();
        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = string.Empty,
            Subject = "Hello",
            Body = "Body text"
        };

        var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreateMessage.Validation", result.Code);
        Assert.Equal("Please provide email, subject, and message body.", result.Message);
        Assert.Null(repository.LastSavedMessage);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldReturnValidationFailure_WhenSubjectIsWhitespace()
    {
        var repository = new FakeMessageRepository();
        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "   ",
            Body = "Body text"
        };

        var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreateMessage.Validation", result.Code);
        Assert.Null(repository.LastSavedMessage);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldReturnValidationFailure_WhenBodyIsEmpty()
    {
        var repository = new FakeMessageRepository();
        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = string.Empty
        };

        var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreateMessage.Validation", result.Code);
        Assert.Null(repository.LastSavedMessage);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldParseSentOnToUtc_WhenValidIsoDateStringIsProvided()
    {
        Message? savedMessage = null;

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (msg, _) =>
            {
                savedMessage = msg;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Body",
            SentOn = "2026-03-15T10:30:00Z"
        };

        await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.NotNull(savedMessage);
        Assert.NotNull(savedMessage!.SentOn);
        Assert.Equal(DateTimeKind.Utc, savedMessage.SentOn!.Value.Kind);
        Assert.Equal(2026, savedMessage.SentOn.Value.Year);
        Assert.Equal(3, savedMessage.SentOn.Value.Month);
        Assert.Equal(15, savedMessage.SentOn.Value.Day);
        Assert.Equal(10, savedMessage.SentOn.Value.Hour);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldLeaveSentOnNull_WhenSentOnIsEmptyString()
    {
        Message? savedMessage = null;

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (msg, _) =>
            {
                savedMessage = msg;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Body",
            SentOn = string.Empty
        };

        await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.NotNull(savedMessage);
        Assert.Null(savedMessage!.SentOn);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldLeaveSentOnNull_WhenSentOnIsInvalidDate()
    {
        Message? savedMessage = null;

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (msg, _) =>
            {
                savedMessage = msg;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Body",
            SentOn = "not-a-date"
        };

        await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.NotNull(savedMessage);
        Assert.Null(savedMessage!.SentOn);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldUseEmptyStringForName_WhenFromIsNull()
    {
        Message? savedMessage = null;

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (msg, _) =>
            {
                savedMessage = msg;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            From = null,
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Body"
        };

        var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(savedMessage);
        Assert.Equal(string.Empty, savedMessage!.Name);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldForwardCancellationTokenToRepository()
    {
        var observedToken = CancellationToken.None;
        using var cts = new CancellationTokenSource();

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (_, token) =>
            {
                observedToken = token;
                return Task.CompletedTask;
            }
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Body"
        };

        await handler.Handle(new CreateMessageCommand.Command(request), cts.Token);

        Assert.Equal(cts.Token, observedToken);
    }

    [Fact]
    public async Task CreateMessageHandler_ShouldReturnUniqueIdPerCall()
    {
        var ids = new List<string>();

        var repository = new FakeMessageRepository
        {
            SaveMessageHandler = (_, _) => Task.CompletedTask
        };

        var handler = new CreateMessageCommand.Handler(repository);

        var request = new CreateMessageRequest
        {
            EmailAddress = "test@example.com",
            Subject = "S",
            Body = "B"
        };

        for (int i = 0; i < 3; i++)
        {
            var result = await handler.Handle(new CreateMessageCommand.Command(request), CancellationToken.None);
            ids.Add(result.Value);
        }

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    // ======================================================================
    // GetMessageQuery.Handler
    // ======================================================================

    [Fact]
    public async Task GetMessageHandler_ShouldReturnSuccess_WhenMessageExists()
    {
        var messageId = Guid.NewGuid();
        var createdOn = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var modifiedOn = createdOn.AddDays(2);
        var sentOn = createdOn.AddHours(1);

        var message = new Message
        {
            Id = messageId,
            Name = "John Doe",
            EmailAddress = "john@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            SentOn = sentOn,
            Status = (int)MessageStatus.Sent,
            AttemptCount = 1,
            MaxRetries = 3,
            LastAttemptedOn = modifiedOn,
            LastErrorMessage = null,
            CreatedOn = createdOn,
            CreatedBy = "system",
            ModifiedOn = modifiedOn,
            ModifiedBy = "processor"
        };

        var repository = new FakeMessageRepository
        {
            GetMessageByIdHandler = (_, _) => Task.FromResult<Message?>(message)
        };

        var handler = new GetMessageQuery.Handler(repository);

        var result = await handler.Handle(new GetMessageQuery.Query(messageId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Code);
        Assert.NotNull(result.Value);
        Assert.Equal(messageId, result.Value.Id);
        Assert.Equal("John Doe", result.Value.Name);
        Assert.Equal("john@example.com", result.Value.EmailAddress);
        Assert.Equal("Test Subject", result.Value.Subject);
        Assert.Equal("Test Body", result.Value.Body);
        Assert.Equal(sentOn, result.Value.SentOn);
        Assert.Equal(MessageStatus.Sent, result.Value.Status);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Equal(3, result.Value.MaxRetries);
        Assert.Equal(modifiedOn, result.Value.LastAttemptedOn);
        Assert.Null(result.Value.LastErrorMessage);
        Assert.Equal(createdOn, result.Value.CreatedOn);
        Assert.Equal("system", result.Value.CreatedBy);
        Assert.Equal(modifiedOn, result.Value.ModifiedOn);
        Assert.Equal("processor", result.Value.ModifiedBy);
    }

    [Fact]
    public async Task GetMessageHandler_ShouldReturnFailure_WhenMessageDoesNotExist()
    {
        var repository = new FakeMessageRepository
        {
            GetMessageByIdHandler = (_, _) => Task.FromResult<Message?>(null)
        };

        var handler = new GetMessageQuery.Handler(repository);

        var result = await handler.Handle(new GetMessageQuery.Query(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("GetMessage.Null", result.Code);
        Assert.Equal("The message with the specified ID was not found", result.Message);
        Assert.NotNull(result.Value);
        Assert.Equal(Guid.Empty, result.Value.Id);
        Assert.Equal(string.Empty, result.Value.Name);
        Assert.Equal(string.Empty, result.Value.EmailAddress);
        Assert.Equal(string.Empty, result.Value.Subject);
    }

    [Fact]
    public async Task GetMessageHandler_ShouldForwardIdToRepository()
    {
        var requestedId = Guid.NewGuid();
        Guid? observedId = null;

        var repository = new FakeMessageRepository
        {
            GetMessageByIdHandler = (id, _) =>
            {
                observedId = id;
                return Task.FromResult<Message?>(null);
            }
        };

        var handler = new GetMessageQuery.Handler(repository);

        await handler.Handle(new GetMessageQuery.Query(requestedId), CancellationToken.None);

        Assert.Equal(requestedId, observedId);
    }

    [Fact]
    public async Task GetMessageHandler_ShouldMapAuditFields_WhenMessageHasDeletionInfo()
    {
        var messageId = Guid.NewGuid();
        var createdOn = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var deletedOn = createdOn.AddDays(7);

        var message = new Message
        {
            Id = messageId,
            Name = "User",
            EmailAddress = "user@example.com",
            Subject = "Deleted",
            Body = "Body",
            Status = (int)MessageStatus.Failed,
            AttemptCount = 3,
            MaxRetries = 3,
            CreatedOn = createdOn,
            CreatedBy = "system",
            DeletedOn = deletedOn,
            DeletedBy = "admin",
            IsDeleted = true
        };

        var repository = new FakeMessageRepository
        {
            GetMessageByIdHandler = (_, _) => Task.FromResult<Message?>(message)
        };

        var handler = new GetMessageQuery.Handler(repository);

        var result = await handler.Handle(new GetMessageQuery.Query(messageId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(deletedOn, result.Value.DeletedOn);
        Assert.Equal("admin", result.Value.DeletedBy);
        Assert.True(result.Value.IsDeleted);
    }

    // ======================================================================
    // GetMessagesQuery.Handler
    // ======================================================================

    [Fact]
    public async Task GetMessagesHandler_ShouldReturnPagedData_WhenPagingParametersAreValid()
    {
        var messages = CreateMessages(5);

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(messages)
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(2, 2), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(messages[2].Id, result.Value[0].Id);
        Assert.Equal(messages[3].Id, result.Value[1].Id);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldUseDefaults_WhenPagingParametersAreNull()
    {
        var messages = CreateMessages(3);

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(messages)
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(null, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.Value.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-5, -1)]
    public async Task GetMessagesHandler_ShouldClampInvalidPagingValues(int pageSize, int pageNumber)
    {
        var messages = CreateMessages(4);

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(messages)
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(pageSize, pageNumber), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(4, result.TotalItems);
        Assert.Equal(4, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(4, result.Value.Count);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldReturnAllData_WhenRequestedPageExceedsRange()
    {
        var messages = CreateMessages(3);

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(messages)
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(2, 5), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.PageNumber);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldHandleEmptyDataSet()
    {
        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(new List<Message>())
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(null, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldMapAllFieldsFromEntity()
    {
        var messageId = Guid.NewGuid();
        var createdOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc);
        var sentOn = createdOn.AddHours(1);
        var lastAttempted = sentOn.AddMinutes(5);

        var message = new Message
        {
            Id = messageId,
            Name = "Alice",
            EmailAddress = "alice@example.com",
            Subject = "Inquiry",
            Body = "Detailed body",
            SentOn = sentOn,
            Status = (int)MessageStatus.Failed,
            AttemptCount = 2,
            MaxRetries = 3,
            LastAttemptedOn = lastAttempted,
            LastErrorMessage = "SMTP timeout",
            CreatedOn = createdOn,
            CreatedBy = "system"
        };

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(new List<Message> { message })
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(null, null), CancellationToken.None);

        var dto = Assert.Single(result.Value);
        Assert.Equal(messageId, dto.Id);
        Assert.Equal("Alice", dto.Name);
        Assert.Equal("alice@example.com", dto.EmailAddress);
        Assert.Equal("Inquiry", dto.Subject);
        Assert.Equal("Detailed body", dto.Body);
        Assert.Equal(sentOn, dto.SentOn);
        Assert.Equal(MessageStatus.Failed, dto.Status);
        Assert.Equal(2, dto.AttemptCount);
        Assert.Equal(3, dto.MaxRetries);
        Assert.Equal(lastAttempted, dto.LastAttemptedOn);
        Assert.Equal("SMTP timeout", dto.LastErrorMessage);
        Assert.Equal(createdOn, dto.CreatedOn);
        Assert.Equal("system", dto.CreatedBy);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldForwardCancellationTokenToRepository()
    {
        var messages = CreateMessages(1);
        var observedToken = CancellationToken.None;
        using var cts = new CancellationTokenSource();

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = token =>
            {
                observedToken = token;
                return Task.FromResult(messages);
            }
        };

        var handler = new GetMessagesQuery.Handler(repository);

        await handler.Handle(new GetMessagesQuery.Query(1, 1), cts.Token);

        Assert.Equal(cts.Token, observedToken);
    }

    [Fact]
    public async Task GetMessagesHandler_ShouldReturnFirstPage_WhenPageNumberIsOne()
    {
        var messages = CreateMessages(6);

        var repository = new FakeMessageRepository
        {
            GetAllMessagesHandler = _ => Task.FromResult(messages)
        };

        var handler = new GetMessagesQuery.Handler(repository);

        var result = await handler.Handle(new GetMessagesQuery.Query(3, 1), CancellationToken.None);

        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(messages[0].Id, result.Value[0].Id);
        Assert.Equal(messages[1].Id, result.Value[1].Id);
        Assert.Equal(messages[2].Id, result.Value[2].Id);
    }

    // ======================================================================
    // Helpers
    // ======================================================================

    private static List<Message> CreateMessages(int count)
    {
        var list = new List<Message>(count);

        for (int i = 1; i <= count; i++)
        {
            list.Add(new Message
            {
                Id = Guid.NewGuid(),
                Name = $"User {i}",
                EmailAddress = $"user{i}@example.com",
                Subject = $"Subject {i}",
                Body = $"Body {i}",
                Status = (int)MessageStatus.Pending,
                AttemptCount = 0,
                MaxRetries = 3,
                CreatedOn = new DateTime(2026, 1, i, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "test"
            });
        }

        return list;
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        public Message? LastSavedMessage { get; private set; }

        public Func<Message, CancellationToken, Task>? SaveMessageHandler { get; init; }
        public Func<Guid, CancellationToken, Task<Message?>>? GetMessageByIdHandler { get; init; }
        public Func<CancellationToken, Task<List<Message>>>? GetAllMessagesHandler { get; init; }

        public Task SaveMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            LastSavedMessage = message;
            return SaveMessageHandler?.Invoke(message, cancellationToken) ?? Task.CompletedTask;
        }

        public Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => GetMessageByIdHandler?.Invoke(id, cancellationToken)
               ?? Task.FromResult<Message?>(null);

        public Task<List<Message>> GetAllMessagesAsync(CancellationToken cancellationToken = default)
            => GetAllMessagesHandler?.Invoke(cancellationToken)
               ?? Task.FromResult(new List<Message>());
    }
}
