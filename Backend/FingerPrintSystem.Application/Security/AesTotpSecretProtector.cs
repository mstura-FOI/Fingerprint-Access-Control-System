using System.Security.Cryptography;
using System.Text;
using FingerPrintSystem.Core.Options;
using Microsoft.Extensions.Options;

namespace FingerPrintSystem.Application.Security;

public class AesTotpSecretProtector(IOptions<TotpProtectorOptions> opt) : ITotpSecretProtector {
    private readonly byte[] _key = Convert.FromBase64String(opt.Value.KeyBase64);

    public string Protect(string secret) {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes(secret);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        return Convert.ToBase64String(nonce.Concat(tag).Concat(ciphertext).ToArray());
    }

    public string Unprotect(string encrypted) {
        var blob = Convert.FromBase64String(encrypted);
        var nonce = blob[..12];
        var tag = blob[12..28];
        var ciphertext = blob[28..];
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}