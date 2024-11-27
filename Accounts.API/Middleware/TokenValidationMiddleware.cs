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
        // Full path matching for exclusion
        var path = context.Request.Path.Value;
        if (path != null && 
            (path.Equals("/api/Account/login", StringComparison.OrdinalIgnoreCase) || 
             path.Equals("/api/Account/logout", StringComparison.OrdinalIgnoreCase) || 
             path.Equals("/api/Account/create", StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context); // Skip validation for login and logout endpoints
            return;
        }

        // Validate the Authorization header for other requests
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            if (_sessionStore.TryGetSession(token, out var session) && session.Expiry >= DateTime.UtcNow)
            {
                context.Items["UserId"] = session.UserId;
                context.Items["Token"] = token; // Store the token for further use
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired session token.");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Authorization header is required.");
            return;
        }

        await _next(context); // Continue processing the request
    }
}