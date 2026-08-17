namespace FingerPrintSystem.Application.Security;

public interface ITotpSecretProtector {
    string Protect(string secret);
    string Unprotect(string encrypted);
}