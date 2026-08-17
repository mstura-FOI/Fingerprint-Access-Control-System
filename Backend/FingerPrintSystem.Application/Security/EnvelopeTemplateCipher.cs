using System.Security.Cryptography;
using System.Text;
using FingerPrintSystem.Core.Options;
using FingerPrintSystem.Infrastructure.Security.ITemplateCipher;
using Microsoft.Extensions.Options;

namespace FingerPrintSystem.Application.Security {
    public class EnvelopeTemplateCipher : ITemplateCipher {
        private const int DekBytes = 32;   // AES-256
        private const int NonceBytes = 12;   // GCM standard
        private const int TagBytes = 16;   // GCM auth tag

        private readonly byte[] _kek;
        private readonly string _kekVersion;

        public EnvelopeTemplateCipher(IOptions<EnvelopeCipherOptions> options) {
            var opts = options.Value;
            if (string.IsNullOrWhiteSpace(opts.KekBase64))
                throw new InvalidOperationException("Envelope:KekBase64 nije postavljen.");

            _kek = Convert.FromBase64String(opts.KekBase64);
            if (_kek.Length != 32)
                throw new InvalidOperationException("KEK mora biti 32 bajta (AES-256).");

            _kekVersion = opts.KekVersion;
        }

        public Task<EncryptedTemplate> EncryptAsync(byte[] plaintext, string aad, CancellationToken ct = default) {
            // 1. Svjež DEK po zapisu.
            byte[] dek = RandomNumberGenerator.GetBytes(DekBytes);
            try {
                byte[] nonce = RandomNumberGenerator.GetBytes(NonceBytes);
                byte[] cipher = new byte[plaintext.Length];
                byte[] tag = new byte[TagBytes];
                byte[] aadBytes = Encoding.UTF8.GetBytes(aad);

                // 2. AES-256-GCM: šifra + auth tag (integritet) u jednom.
                using (var gcm = new AesGcm(dek, TagBytes))
                    gcm.Encrypt(nonce, plaintext, cipher, tag, aadBytes);

                // 3. Omota DEK KEK-om (DEV: lokalni AES-GCM; PROD: Key Vault WrapKey).
                var (wrappedDek, wrapNonce) = WrapDek(dek);

                byte[] wrapped = new byte[wrapNonce.Length + wrappedDek.Length];
                Buffer.BlockCopy(wrapNonce, 0, wrapped, 0, wrapNonce.Length);
                Buffer.BlockCopy(wrappedDek, 0, wrapped, wrapNonce.Length, wrappedDek.Length);

                return Task.FromResult(new EncryptedTemplate(
                    Ciphertext: cipher,
                    Nonce: nonce,
                    Tag: tag,
                    WrappedDek: wrapped,
                    KekKeyId: _kekVersion));
            } finally {
                CryptographicOperations.ZeroMemory(dek);
            }
        }

        public Task<byte[]> DecryptAsync(EncryptedTemplate e, string aad, CancellationToken ct = default) {
            byte[] wrapNonce = e.WrappedDek[..NonceBytes];
            byte[] wrappedDek = e.WrappedDek[NonceBytes..];
            byte[] dek = UnwrapDek(wrappedDek, wrapNonce);
            try {
                byte[] plain = new byte[e.Ciphertext.Length];
                byte[] aadBytes = Encoding.UTF8.GetBytes(aad);

                using (var gcm = new AesGcm(dek, TagBytes))
                    gcm.Decrypt(e.Nonce, e.Ciphertext, e.Tag, plain, aadBytes);

                return Task.FromResult(plain);
            } finally {
                CryptographicOperations.ZeroMemory(dek);
            }
        }

        // ---- KEK operacije. OVDJE se u produkciji ubacuje Key Vault. ----
        private (byte[] wrapped, byte[] nonce) WrapDek(byte[] dek) {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceBytes);
            byte[] wrapped = new byte[dek.Length + TagBytes];
            byte[] cipher = new byte[dek.Length];
            byte[] tag = new byte[TagBytes];

            using (var gcm = new AesGcm(_kek, TagBytes))
                gcm.Encrypt(nonce, dek, cipher, tag);

            Buffer.BlockCopy(cipher, 0, wrapped, 0, cipher.Length);
            Buffer.BlockCopy(tag, 0, wrapped, cipher.Length, TagBytes);
            return (wrapped, nonce);
        }

        private byte[] UnwrapDek(byte[] wrapped, byte[] nonce) {
            byte[] cipher = wrapped[..^TagBytes];
            byte[] tag = wrapped[^TagBytes..];
            byte[] dek = new byte[cipher.Length];

            using (var gcm = new AesGcm(_kek, TagBytes))
                gcm.Decrypt(nonce, cipher, tag, dek);

            return dek;
        }
    }
}
