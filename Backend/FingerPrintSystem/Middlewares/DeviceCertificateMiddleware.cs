using System.Security.Cryptography.X509Certificates;

namespace FingerPrintSystem.WebApi.Middlewares;

public class DeviceCertificateMiddleware(RequestDelegate next) {
    private static readonly X509Certificate2 DeviceCa =
        X509CertificateLoader.LoadCertificateFromFile(
            Path.Combine(AppContext.BaseDirectory, "device-ca.crt"));

    public async Task Invoke(HttpContext context) {
        if (context.Request.Path.StartsWithSegments("/api/device")) {
            var cert = context.Connection.ClientCertificate;

            if (cert is null || !ChainsToOurCa(cert)) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Nevaljan ili nedostajući device certifikat.");
                return;
            }
        }

        await next(context);
    }

    private static bool ChainsToOurCa(X509Certificate2 cert) {
        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(DeviceCa);
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return chain.Build(cert);
    }
}