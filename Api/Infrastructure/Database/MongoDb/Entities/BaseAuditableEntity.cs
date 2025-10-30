using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

public class BaseAuditableEntity : Entity
{
    [Field("createdon")]
    public string CreatedOn { get; set; }

    [Field("createdby")]
    public string CreatedBy { get; set; } = string.Empty;

    [Field("modifiedon")]
    public string? ModifiedOn { get; set; }

    [Field("modifiedby")]
    public string? ModifiedBy { get; set; }

    [Field("deletedon")]
    public string? DeletedOn { get; set; }

    [Field("deletedby")]
    public string? DeletedBy { get; set; }

    [Field("isdeleted")]
    public bool IsDeleted { get; set; }
}
