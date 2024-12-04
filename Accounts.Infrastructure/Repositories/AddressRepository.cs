using Microsoft.EntityFrameworkCore;
using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;

namespace Accounts.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Address> GetOrCreateAddressAsync(Address address)
        {
            var existingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.StreetNumber == address.StreetNumber &&
                    a.StreetName == address.StreetName &&
                    a.CityPostalCode == address.CityPostalCode);

            if (existingAddress != null)
            {
                return existingAddress;
            }

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            return address;
        }
    }
}