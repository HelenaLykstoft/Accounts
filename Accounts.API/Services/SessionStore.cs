public class SessionStore
{
    private readonly Dictionary<string, Session> _sessions = new();

    public virtual void AddSession(string token, Guid userId, DateTime expiry)
    {
        _sessions[token] = new Session { UserId = userId, Expiry = expiry };
    }

    public virtual bool TryGetSession(string token, out Session? session)
    {
        return _sessions.TryGetValue(token, out session);
    }
    
    public virtual bool RemoveSession(string token)
    {
        return _sessions.Remove(token);
    }
    
    public virtual IEnumerable<Session> GetAllSessions()
    {
        return _sessions.Values;
    }
}

public class Session
{
    public Guid UserId { get; set; }
    public DateTime Expiry { get; set; }
}