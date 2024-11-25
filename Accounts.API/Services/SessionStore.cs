public class SessionStore
{
    private readonly Dictionary<string, Session> _sessions = new();

    public void AddSession(string token, Guid userId, DateTime expiry)
    {
        _sessions[token] = new Session { UserId = userId, Expiry = expiry };
    }

    public bool TryGetSession(string token, out Session? session)
    {
        return _sessions.TryGetValue(token, out session);
    }
    
    public bool RemoveSession(string token)
    {
        return _sessions.Remove(token);
    }
}

public class Session
{
    public Guid UserId { get; set; }
    public DateTime Expiry { get; set; }
}