using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Posts.Infrastructure;

public class PostRepository : IPostRepository
{
	private readonly AppDbContext _context;

	public PostRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<Post>> FilterPostsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		var connection = _context.Database.GetDbConnection();

		var sql = @"
			SELECT
				""Id"",
				""Title"",
				""Heading"",
				""Description"",
				""Content"",
				""Author"",
				""UserId"",
				""Category"",
				""TotalViews"",
				""Likes"",
				""PostedOn"",
				""CreatedOn"",
				""CreatedBy"",
				""ModifiedOn"",
				""ModifiedBy"",
				""DeletedOn"",
				""DeletedBy"",
				""IsDeleted""
			FROM ""Posts""
			WHERE ""IsDeleted"" = false AND ""UserId"" = @UserId
			ORDER BY ""PostedOn"" DESC";

		var rows = await connection.QueryAsync<PostRow>(
			new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

		return rows.Select(MapRowToPost).ToList();
	}

	public async Task<List<Post>> GetAllPostsAsync(CancellationToken cancellationToken = default)
	{
		var connection = _context.Database.GetDbConnection();

		var sql = @"
			SELECT
				""Id"",
				""Title"",
				""Heading"",
				""Description"",
				""Content"",
				""Author"",
				""UserId"",
				""Category"",
				""TotalViews"",
				""Likes"",
				""PostedOn"",
				""CreatedOn"",
				""CreatedBy"",
				""ModifiedOn"",
				""ModifiedBy"",
				""DeletedOn"",
				""DeletedBy"",
				""IsDeleted""
			FROM ""Posts""
			WHERE ""IsDeleted"" = false
			ORDER BY ""PostedOn"" DESC";

		var rows = await connection.QueryAsync<PostRow>(
			new CommandDefinition(sql, cancellationToken: cancellationToken));

		return rows.Select(MapRowToPost).ToList();
	}

	public async Task<Post?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var connection = _context.Database.GetDbConnection();

		var sql = @"
			SELECT
				""Id"",
				""Title"",
				""Heading"",
				""Description"",
				""Content"",
				""Author"",
				""UserId"",
				""Category"",
				""TotalViews"",
				""Likes"",
				""PostedOn"",
				""CreatedOn"",
				""CreatedBy"",
				""ModifiedOn"",
				""ModifiedBy"",
				""DeletedOn"",
				""DeletedBy"",
				""IsDeleted""
			FROM ""Posts""
			WHERE ""Id"" = @Id AND ""IsDeleted"" = false";

		var row = await connection.QuerySingleOrDefaultAsync<PostRow>(
			new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

		return row == null ? null : MapRowToPost(row);
	}

	public async Task SavePostAsync(Post post, CancellationToken cancellationToken = default)
	{
		await _context.Posts.AddAsync(post, cancellationToken);
		_context.SaveChanges(post.CreatedBy, cancellationToken);
	}

	public Task UpdatePostAsync(Post post, CancellationToken cancellationToken = default)
	{
		_context.Posts.Update(post);
		_context.SaveChanges(post.ModifiedBy ?? "System", cancellationToken);

		return Task.CompletedTask;
	}

	private static Post MapRowToPost(PostRow row)
	{
		return new Post
		{
			Id = row.Id,
			Title = row.Title,
			Heading = row.Heading,
			Description = row.Description,
			Content = row.Content,
			Author = row.Author,
			UserId = row.UserId,
			Category = row.Category,
			TotalViews = row.TotalViews,
			Likes = row.Likes?.ToList() ?? [],
			PostedOn = row.PostedOn,
			CreatedOn = row.CreatedOn,
			CreatedBy = row.CreatedBy,
			ModifiedOn = row.ModifiedOn,
			ModifiedBy = row.ModifiedBy,
			DeletedOn = row.DeletedOn,
			DeletedBy = row.DeletedBy,
			IsDeleted = row.IsDeleted
		};
	}

	private sealed class PostRow
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Heading { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public int TotalViews { get; set; }
		public string[]? Likes { get; set; }
		public DateTime PostedOn { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public DateTime? ModifiedOn { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime? DeletedOn { get; set; }
		public string? DeletedBy { get; set; }
		public bool IsDeleted { get; set; }
	}
}
