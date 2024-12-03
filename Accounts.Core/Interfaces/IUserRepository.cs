
using Accounts.Core.Entities;

namespace Accounts.Core.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
    }
}