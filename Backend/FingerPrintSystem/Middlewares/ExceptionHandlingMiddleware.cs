using System.Text.Json;

namespace FingerPrintSystem.WebApi.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger) {
    public async Task Invoke(HttpContext context) {
        try {
            await next(context);
        } catch (InvalidOperationException ex) {
            logger.LogWarning(ex, "Poslovna greška");
            await WriteProblem(context, StatusCodes.Status400BadRequest, ex.Message);
        } catch (UnauthorizedAccessException ex) {
            logger.LogWarning(ex, "Neovlašteno");
            await WriteProblem(context, StatusCodes.Status403Forbidden, "Nemate ovlast za ovu radnju.");
        } catch (Exception ex) {
            logger.LogError(ex, "Neočekivana greška");
            await WriteProblem(context, StatusCodes.Status500InternalServerError,
                "Server trenutačno ne može obraditi zahtjev.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string detail) {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new {
            title = status switch {
                400 => "Neispravan zahtjev",
                403 => "Zabranjeno",
                _ => "Greška"
            },
            status,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}