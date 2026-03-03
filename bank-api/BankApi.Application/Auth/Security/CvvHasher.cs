using System.Security.Cryptography;

namespace BankApi.Application.Security;

public static class CvvHasher
{
    private const int SaltSize = 16;       
    private const int KeySize = 32;        
    private const int Iterations = 100_000;

    public static (byte[] hash, byte[] salt) Hash(string cvv)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: cvv,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256
        );

        var hash = pbkdf2.GetBytes(KeySize);
        return (hash, salt);
    }

    public static bool Verify(string cvv, byte[] hash, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: cvv,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256
        );

        var computed = pbkdf2.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}
