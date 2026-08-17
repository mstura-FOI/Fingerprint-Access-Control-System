namespace FingerPrintSystem.Infrastructure.Security.ITemplateCipher;

public interface ITemplateCipher
{
    Task<EncryptedTemplate> EncryptAsync(byte[] plaintext, string aad, CancellationToken ct = default);
    Task<byte[]> DecryptAsync(EncryptedTemplate encrypted, string aad, CancellationToken ct = default);
}