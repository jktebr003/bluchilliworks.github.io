using System;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Posts.Domain;

public class Post : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Heading { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
    public string UserId { get; set; }
    public string Category { get; set; }
    public int TotalViews { get; set; }
    public List<string> Likes { get; set; } = [];
    public DateTime PostedOn { get; set; }
    // public List<Comment> Comments { get; set; } = new List<Comment>();
}
