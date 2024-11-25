using Accounts.API.Services;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SessionStore _sessionStore;
    private readonly IServiceProvider _serviceProvider;

    public TokenValidationMiddleware(RequestDelegate next, SessionStore sessionStore, IServiceProvider serviceProvider)
    {
        _next = next;
        _sessionStore = sessionStore;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            if (_sessionStore.TryGetSession(token, out var session) && session.Expiry >= DateTime.UtcNow)
            {
                context.Items["UserId"] = session.UserId;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
                    var username = await accountService.GetUsernameByIdAsync(session.UserId);
                    if (!string.IsNullOrEmpty(username))
                    {
                        context.Items["Username"] = username;
                    }
                }
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