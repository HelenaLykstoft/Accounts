using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;

namespace Accounts.Infrastructure.Repositories
{
    public class ContactInfoRepository : IContactInfoRepository
    {
        private readonly AppDbContext _context;

        public ContactInfoRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task AddContactInfoAsync(ContactInfo contactInfo)
        {
            await _context.ContactInfos.AddAsync(contactInfo);
            await _context.SaveChangesAsync();
        }
    }
}
