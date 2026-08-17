namespace FingerPrintSystem.Application.Services.EnrollmentService;

public interface IEnrollmentService
{
    /// <summary>
    ///     Dovrši registraciju otiska. Kod je autorizacija cijele operacije —
    ///     iz njega se izvodi korisnik (ne prima se izvana). Template je već
    ///     kombiniran na AS608 (5 uzoraka → jedan template) prije nego stigne.
    /// </summary>
    Task CompleteAsync(string code, byte[] rawTemplate, CancellationToken ct = default);
}