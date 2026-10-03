using System.Security.Cryptography;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.Security;

/// <summary>
/// Chuẩn băm mật khẩu PBKDF2 với Cryptographic Salt ngẫu nhiên và 100.000 vòng lặp HMAC-SHA256.
/// Ngăn chặn tấn công Rainbow Table và Brute-force, tuân thủ chặt chẽ khuyến nghị OWASP/NIST.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // 128-bit salt
    private const int KeySize = 32;        // 256-bit subkey
    private const int Iterations = 100000; // 100,000 vòng lặp
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword)) return string.Empty;

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // Định dạng lưu: Salt (16B) + Hash (32B) = 48B -> Base64
        byte[] result = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, KeySize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            byte[] hashBytes = Convert.FromBase64String(passwordHash);
            if (hashBytes.Length != SaltSize + KeySize)
                return false;

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            byte[] expectedHash = new byte[KeySize];
            Buffer.BlockCopy(hashBytes, SaltSize, expectedHash, 0, KeySize);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                plainPassword,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            return false;
        }
    }
}