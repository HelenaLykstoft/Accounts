using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Repositories
{

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId) =>
            await _context.Users.FindAsync(userId);

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _context.Users.Include(u => u.LoginInformation)
                                .FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> UsernameExistsAsync(string username) =>
            await _context.Users.AnyAsync(u => u.Username == username);

        public async Task<int> GetUsersCountAsync() =>
            await _context.Users.CountAsync();

        public async Task AddUserAsync(User user)
        {
            if (user.ContactInfo == null || user.UserType == null)
            {
                throw new ArgumentException("User must have valid ContactInfo and UserType.");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AdminAccountExistsAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserTypeId == 3);
            return user != null;
        }
    }
}