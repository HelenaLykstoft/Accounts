using Microsoft.EntityFrameworkCore;
using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;

namespace Accounts.Infrastructure.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly AppDbContext _context;

        public CityRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<City> GetOrCreateCityAsync(int postalCode, string cityName)
        {
            var existingCity = await _context.Cities.FirstOrDefaultAsync(c => c.PostalCode == postalCode);
            if (existingCity != null)
            {
                return existingCity;
            }

            var city = new City
            {
                PostalCode = postalCode,
                Name = cityName
            };
            await _context.Cities.AddAsync(city);
            await _context.SaveChangesAsync();
            return city;
        }
    }
}
