using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ESCL8.Api.Security;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _cfg;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration cfg)
    {
        _next = next;
        _cfg = cfg;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        // Public endpoints
        if (path.StartsWith("/health") ||
            (path.StartsWith("/api/v1/incidents") && method == "POST"))
        {
            await _next(context);
            return;
        }

        var dispatcherKey = _cfg["Security:DispatcherApiKey"];
        var ambulanceKey = _cfg["Security:AmbulanceApiKey"];
        var provided = context.Request.Headers["X-API-KEY"].ToString();

        bool dispatcherRoute =
            path.StartsWith("/api/v1/incidents/") &&
            (path.Contains("/assign")
          || path.Contains("/reassign")
          || path.Contains("/unassign")
          || path.Contains("/retry-auto-assign"));

        bool ambulanceRoute =
            path.StartsWith("/api/v1/ambulances");

        bool sharedRoute =
            path.StartsWith("/api/v1/incidents/") &&
            (path.Contains("/status") || path.Contains("/location"));

        bool ok =
            dispatcherRoute ? provided == dispatcherKey
          : ambulanceRoute ? provided == ambulanceKey
          : sharedRoute ? (provided == dispatcherKey || provided == ambulanceKey)
          : true;

        if (!ok)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        await _next(context);
    }
}