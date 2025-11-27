namespace Api.Services;

public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a password using Argon2id algorithm
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>The hashed password with salt embedded</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hashed password
    /// </summary>
    /// <param name="hashedPassword">The hashed password to verify against</param>
    /// <param name="providedPassword">The plaintext password to verify</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
