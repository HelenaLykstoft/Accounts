using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Infrastructure.Repositories
{
    public class LoginInfoRepository : ILoginInfoRepository
    {
        private readonly AppDbContext _context;

        public LoginInfoRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task AddLoginInfoAsync(LoginInformation loginInfo)
        {
            await _context.LoginInformations.AddAsync(loginInfo);
            await _context.SaveChangesAsync();
        }
    }
}
