using Accounts.Core.Entities;
using Accounts.Core.Interfaces;
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

        public async Task AddUserAsync(User user)
        {
            if (user.ContactInfo == null || user.UserType == null)
            {
                throw new ArgumentException("User must have valid ContactInfo and UserType.");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}