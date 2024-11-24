public class SessionStore
{
    private readonly Dictionary<string, (Guid UserId, DateTime Expiry)> _sessions = new();

    public bool TryGetSession(string token, out (Guid UserId, DateTime Expiry) session)
    {
        return _sessions.TryGetValue(token, out session);
    }

    public void AddSession(string token, Guid userId, DateTime expiry)
    {
        _sessions[token] = (userId, expiry);
    }

    public void RemoveSession(string token)
    {
        _sessions.Remove(token);
    }
}