using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("packages")]
public class Package : BaseAuditableEntity
{
    [Field("name")]
    public string? Name { get; set; }

    [Field("description")]
    public string? Description { get; set; }

    [Field("code")]
    public string? Code { get; set; }

    [Field("price")]
    public string? Price { get; set; }

    [Field("packageType")]
    public int Type { get; set; }
}
