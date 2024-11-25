
using Accounts.Domain.Entities;

namespace Accounts.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
    }
}