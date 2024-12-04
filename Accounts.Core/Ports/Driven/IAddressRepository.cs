using Accounts.Core.Entities;
namespace Accounts.Core.Ports.Driven
{
    public interface IAddressRepository
    {
        Task<Address> GetOrCreateAddressAsync(Address address);
    }
}
