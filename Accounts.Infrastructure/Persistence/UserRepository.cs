using Accounts.Domain.Entities;
using Accounts.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Corrected DbSet usage: _context.User -> _context.Users
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            // Ensure ContactInfo and UserType references are valid before adding the User
            if (user.ContactInfo == null || user.UserType == null)
            {
                throw new ArgumentException("User must have valid ContactInfo and UserType.");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}