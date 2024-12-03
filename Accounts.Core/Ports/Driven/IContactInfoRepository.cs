using Accounts.Core.Entities;

namespace Accounts.Core.Ports.Driven
{
    public interface IContactInfoRepository
    {
        Task AddContactInfoAsync(ContactInfo contactInfo);
    }
}
