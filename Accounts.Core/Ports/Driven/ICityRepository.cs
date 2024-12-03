using Accounts.Core.Entities;

namespace Accounts.Core.Ports.Driven
{
    public interface ICityRepository
    {
        Task<City> GetOrCreateCityAsync(int postalCode, string cityName);
    }
}