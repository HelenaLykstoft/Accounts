using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;

namespace Accounts.Infrastructure.Persistence
{
    public class SessionStore : ISessionStore
    {
        private readonly Dictionary<string, Session> _sessions = new();

        public virtual void AddSession(string token, Guid userId, DateTime expiry)
        {
            _sessions[token] = new Session { UserId = userId, Expiry = expiry };
        }

        public virtual bool TryGetSession(string token, out Session? session)
        {
            if (_sessions.TryGetValue(token, out session))
            {
                if (session.Expiry > DateTime.UtcNow)
                {
                    return true; // Valid session
                }
                else
                {
                    // Remove expired session
                    _sessions.Remove(token);
                    session = null;
                    return false;
                }
            }

            session = null;
            return false; // No session found
        }


        public virtual bool RemoveSession(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            return _sessions.Remove(token);
        }


        public virtual IEnumerable<Session> GetAllSessions()
        {
            return _sessions.Values;
        }

        public virtual bool HasActiveSessionForUser(Guid userId)
        {
            // Iterate through all sessions to check if the user already has an active session
            foreach (var session in _sessions.Values)
            {
                if (session.UserId == userId && session.Expiry > DateTime.UtcNow)
                {
                    return true; // The user has an active session
                }
            }
            return false; // No active session found for the user
        }
    }
}