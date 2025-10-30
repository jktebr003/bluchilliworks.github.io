using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("posts")]
public class Post : BaseAuditableEntity
{
    [Field("title")]
    public string? Title { get; set; }

    [Field("heading")]
    public string? Heading { get; set; }

    [Field("description")]
    public string? Description { get; set; }

    [Field("content")]
    public string? Content { get; set; }

    [Field("author")]
    public string? Author { get; set; }

    [Field("userid")]
    public string? UserId { get; set; }

    [Field("category")]
    public string? Category { get; set; }

    [Field("views")]
    public int? TotalViews { get; set; }

    [Field("likes")]
    public List<string> Likes { get; set; } = new List<string>();

    //[Field("coverimage")]
    //public Image? CoverImage { get; set; }

    [Field("postdate")]
    public string? PostedOn { get; set; }

    //[Field("editdate")]
    //public DateTime? DateEdited { get; set; }

    [Field("comments")]
    public List<Comment> Comments { get; set; } = new List<Comment>();

    //[Field("images")]
    //public List<Image> Images { get; set; } = new List<Image>();
}
