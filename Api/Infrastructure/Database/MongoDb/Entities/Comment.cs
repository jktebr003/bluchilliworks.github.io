using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("comments")]
public class Comment : BaseAuditableEntity
{
    [Field("postid")]
    public string? PostId { get; set; }

    [Field("content")]
    public string? CommentContent { get; set; }

    [Field("commentdate")]
    public string? DateCommented { get; set; }

    [Field("author")]
    public string? Author { get; set; }
}
