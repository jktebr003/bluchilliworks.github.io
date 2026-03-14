using MudBlazorWeb.Features.Audits.Application;
using MudBlazorWeb.Features.Audits.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

using Xunit;

namespace MudBlazorWeb.Features.Audits.Tests;

public class AuditHandlerTests
{
    [Fact]
    public async Task GetAuditsHandler_ShouldReturnPagedData_WhenPagingParametersAreValid()
    {
        var audits = CreateAudits(5);
        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(audits)
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(2, 2), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(audits[2].Id, result.Value[0].Id);
        Assert.Equal(audits[3].Id, result.Value[1].Id);
    }

    [Fact]
    public async Task GetAuditsHandler_ShouldUseDefaults_WhenPagingParametersAreNull()
    {
        var audits = CreateAudits(3);
        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(audits)
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(null, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.Value.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-10, -2)]
    public async Task GetAuditsHandler_ShouldClampInvalidPagingValues(int pageSize, int pageNumber)
    {
        var audits = CreateAudits(4);
        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(audits)
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(pageSize, pageNumber), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(4, result.TotalItems);
        Assert.Equal(4, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(4, result.Value.Count);
    }

    [Fact]
    public async Task GetAuditsHandler_ShouldReturnAllData_WhenRequestedPageExceedsRange()
    {
        var audits = CreateAudits(3);
        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(audits)
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(2, 5), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.PageNumber);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task GetAuditsHandler_ShouldHandleEmptyDataSet()
    {
        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(new List<Audit>())
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(null, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAuditsHandler_ShouldMapAllFieldsFromEntity()
    {
        var createdDate = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        var audit = new Audit
        {
            Id = Guid.NewGuid(),
            AuditType = "Update",
            AuditUser = "auditor@example.com",
            TableName = "Packages",
            KeyValues = "{\"Id\":\"1\"}",
            OldValues = "{\"Name\":\"Old\"}",
            NewValues = "{\"Name\":\"New\"}",
            ChangedColumns = "[\"Name\"]",
            CreatedBy = "system",
            CreatedDate = createdDate
        };

        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = _ => Task.FromResult(new List<Audit> { audit })
        };

        var handler = new GetAuditsQuery.Handler(repository);

        var result = await handler.Handle(new GetAuditsQuery.Query(null, null), CancellationToken.None);

        var dto = Assert.Single(result.Value);
        Assert.Equal(audit.Id, dto.Id);
        Assert.Equal("Update", dto.AuditType);
        Assert.Equal("auditor@example.com", dto.AuditUser);
        Assert.Equal("Packages", dto.TableName);
        Assert.Equal("{\"Id\":\"1\"}", dto.KeyValues);
        Assert.Equal("{\"Name\":\"Old\"}", dto.OldValues);
        Assert.Equal("{\"Name\":\"New\"}", dto.NewValues);
        Assert.Equal("[\"Name\"]", dto.ChangedColumns);
        Assert.Equal("system", dto.CreatedBy);
        Assert.Equal(createdDate, dto.CreatedDate);
    }

    [Fact]
    public async Task GetAuditsHandler_ShouldForwardCancellationTokenToRepository()
    {
        var audits = CreateAudits(1);
        var observedToken = CancellationToken.None;
        using var cts = new CancellationTokenSource();

        var repository = new FakeAuditRepository
        {
            GetAllAuditsHandler = token =>
            {
                observedToken = token;
                return Task.FromResult(audits);
            }
        };

        var handler = new GetAuditsQuery.Handler(repository);

        await handler.Handle(new GetAuditsQuery.Query(1, 1), cts.Token);

        Assert.Equal(cts.Token, observedToken);
    }

    private static List<Audit> CreateAudits(int count)
    {
        var list = new List<Audit>(count);

        for (int i = 1; i <= count; i++)
        {
            list.Add(new Audit
            {
                Id = Guid.NewGuid(),
                AuditType = i % 2 == 0 ? "Update" : "Create",
                AuditUser = $"user{i}@example.com",
                TableName = "Messages",
                KeyValues = $"{{\"Id\":\"{i}\"}}",
                ChangedColumns = "[\"Status\"]",
                CreatedBy = "test",
                CreatedDate = new DateTime(2026, 3, i, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        return list;
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public Func<CancellationToken, Task<List<Audit>>>? GetAllAuditsHandler { get; init; }

        public Task<List<Audit>> GetAllAuditsAsync(CancellationToken cancellationToken = default)
            => GetAllAuditsHandler?.Invoke(cancellationToken)
               ?? Task.FromResult(new List<Audit>());
    }
}