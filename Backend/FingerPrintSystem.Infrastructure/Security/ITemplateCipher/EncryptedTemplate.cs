namespace FingerPrintSystem.Infrastructure.Security.ITemplateCipher;

public record EncryptedTemplate(
    byte[] Ciphertext,
    byte[] Nonce,
    byte[] Tag,
    byte[] WrappedDek,
    string KekKeyId);