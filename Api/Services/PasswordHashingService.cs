using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services;

/// <summary>
/// Service for hashing and verifying passwords using Argon2id algorithm
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    // Argon2id parameters - recommended for security
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 4; // Time cost
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 1; // Number of threads

    /// <summary>
    /// Hashes a password using Argon2id algorithm with embedded salt
    /// Format: {salt}{hash} both base64 encoded
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate a random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with Argon2id
        byte[] hash = HashPasswordWithSalt(password, salt);

        // Combine salt and hash, then encode to base64
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            return false;

        try
        {
            // Decode the stored hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            // Extract salt and hash
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            byte[] storedHash = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Hash the provided password with the same salt
            byte[] providedHash = HashPasswordWithSalt(providedPassword, salt);

            // Compare hashes using constant-time comparison
            return CryptographicOperations.FixedTimeEquals(storedHash, providedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Internal method to hash password with a specific salt using Argon2id
    /// </summary>
    private byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}
