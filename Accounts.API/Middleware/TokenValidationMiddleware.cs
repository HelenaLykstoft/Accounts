public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SessionStore _sessionStore;

    public TokenValidationMiddleware(RequestDelegate next, SessionStore sessionStore)
    {
        _next = next;
        _sessionStore = sessionStore;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            if (_sessionStore.TryGetSession(token, out var session) && session.Expiry >= DateTime.UtcNow)
            {
                context.Items["UserId"] = session.UserId;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired session token.");
                return;
            }
        }

        await _next(context);
    }
}