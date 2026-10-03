using System.Security.Cryptography;
using System.Text;
using Payroll.UseCases.PluginInterfaces;

namespace Payroll.Plugins.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword)) return string.Empty;
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        string computedHash = HashPassword(plainPassword);
        return string.Equals(computedHash, passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}
