namespace VisualizationDSA.WebApi.Middlewares;

/// <summary>
/// O6: Security headers middleware — adds HTTP security headers to every response.
/// CSP is permissive in Development (allows localhost), strict in Production.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // ── Anti-sniffing ──
        headers["X-Content-Type-Options"] = "nosniff";

        // ── Clickjacking protection ──
        headers["X-Frame-Options"] = "DENY";

        // ── Legacy XSS filter ──
        headers["X-XSS-Protection"] = "1; mode=block";

        // ── Hide server identity ──
        headers["Server"] = "VisualizationDSA";

        // ── Referrer policy ──
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // ── Permissions policy ──
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // ── Cross-Origin Resource Policy ──
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // ── Cross-Origin Opener Policy ──
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        // ── Cross-Origin Embedder Policy ──
        headers["Cross-Origin-Embedder-Policy"] = "require-corp";

        // ── DNS Prefetch Control ──
        headers["X-DNS-Prefetch-Control"] = "off";

        // ── O6: HSTS — only in Production (HTTPS required) ──
        if (!_env.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // ── Content-Security-Policy ──
        var connectSrc = _env.IsDevelopment()
            ? "connect-src 'self' http://localhost:* ws://localhost:* https://cdn.jsdelivr.net;"
            : "connect-src 'self' https://*.visualizationdsa.dev wss://*.visualizationdsa.dev https://cdn.jsdelivr.net;";

        var imgSrc = "img-src 'self' data: blob: https: res.cloudinary.com;";

        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-eval' 'unsafe-inline' blob: https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com data:; " +
            imgSrc + " " +
            connectSrc + " " +
            "worker-src 'self' blob:; " +
            "trusted-types 'self' https://cdn.jsdelivr.net; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none';";

        await _next(context);
    }
}


public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
