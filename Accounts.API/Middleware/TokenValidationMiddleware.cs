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
        // Full path matching for exclusion (login, logout, create)
        var path = context.Request.Path.Value;
        if (path != null && 
            (path.Equals("/api/Account/login", StringComparison.OrdinalIgnoreCase) || 
             path.Equals("/api/Account/logout", StringComparison.OrdinalIgnoreCase) || 
             path.Equals("/api/Account/create", StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context); // Skip validation for login, logout, and create endpoints
            return;
        }

        // Validate the Authorization header for other requests
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            if (_sessionStore.TryGetSession(token, out var session) && session.Expiry >= DateTime.UtcNow)
            {
                // Ensure only one active token per user
                if (HasActiveSessionForUser(session.UserId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("This user is already logged in with another token.");
                    return;
                }

                // Store user ID and token for further use
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

    public virtual bool HasActiveSessionForUser(Guid userId)
    {
        // Iterate through all sessions to check if a user already has an active session
        foreach (var session in _sessionStore.GetAllSessions())
        {
            if (session.UserId == userId && session.Expiry > DateTime.UtcNow)
            {
                return true; // The user has an active session
            }
        }
        return false; // No active session found for the user
    }
}
