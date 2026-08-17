using System.Security.Cryptography;
using System.Text;

namespace FingerPrintSystem.Infrastructure.Security;

public static class HashingHelper {
    public static string HashHighEntropy(string value) {
        ArgumentException.ThrowIfNullOrEmpty(value);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public static string HashLowEntropy(string value, byte[] pepper) {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (pepper is null || pepper.Length == 0)
            throw new InvalidOperationException(
                "Pepper je obavezan za niskoentropijske vrijednosti.");

        var bytes = HMACSHA256.HashData(pepper, Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public static bool FixedTimeEquals(string hashA, string hashB) {
        var a = Encoding.UTF8.GetBytes(hashA);
        var b = Encoding.UTF8.GetBytes(hashB);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}