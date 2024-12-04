using Accounts.Core.Entities;

namespace Accounts.Core.Ports.Driven
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<int> GetUsersCountAsync();
        Task AddUserAsync(User user);
        Task<bool> AdminAccountExistsAsync();
    }
}