using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("users")]
public class User : BaseAuditableEntity
{
    [Field("name")]
    public string? Name { get; set; }

    [Field("firstname")]
    public string? FirstName { get; set; }

    [Field("lastname")]
    public string? LastName { get; set; }

    [Field("username")]
    public string? Username { get; set; }

    [Field("email")]
    public string? EmailAddress { get; set; }

    [Field("telephone")]
    public string? TelephoneNumber { get; set; }

    [Field("mobile")]
    public string? MobileNumber { get; set; }

    [Field("hashedpassword")]
    public string? HashedPassword { get; set; }

    [Field("emailverified")]
    public bool EmailVerified { get; set; } = false;

    [Field("emailverificationtoken")]
    public string? EmailVerificationToken { get; set; }

    [Field("emailverificationtokenexpiry")]
    public string? EmailVerificationTokenExpiry { get; set; }

    [Field("passwordresettoken")]
    public string? PasswordResetToken { get; set; }

    [Field("passwordresettokenexpiry")]
    public string? PasswordResetTokenExpiry { get; set; }

    [Field("gender")]
    public string? Gender { get; set; }

    [Field("birthdate")]
    public string? DateOfBirth { get; set; }

    //static User()
    //{
    //    DB.Index<User>()
    //      .Key(b => b.Email, KeyType.Ascending)
    //      .CreateAsync();
    //}

    [Field("package")]
    public Package? Package { get; set; }

    //[Field("career")]
    //public Career? Career { get; set; }

    //[Field("education")]
    //public Education? Education { get; set; }

    [Field("jobs")]
    public List<Job>? Jobs { get; set; } = null;

    [Field("qualifications")]
    public List<Qualification>? Qualifications { get; set; } = null;

    [Field("certifications")]
    public List<Certification>? Certifications { get; set; } = null;

    [Field("avatar")]
    public int? Avatar { get; set; }

    [Field("usertype")]
    public int? UserType { get; set; }

    [Field("skills")]
    public string? Skills { get; set; }

    [Field("hobbies")]
    public string? Hobbies { get; set; }
}
