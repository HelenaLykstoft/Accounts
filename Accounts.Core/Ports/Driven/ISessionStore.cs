using Accounts.Core.Entities;

namespace Accounts.Core.Ports.Driven
{
    public interface ISessionStore
    {
        void AddSession(string token, Guid userId, DateTime expiry);
        bool TryGetSession(string token, out Session? session);
        bool RemoveSession(string token);
        IEnumerable<Session> GetAllSessions();
        bool HasActiveSessionForUser(Guid userId);
    }
}