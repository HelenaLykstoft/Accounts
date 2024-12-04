using Accounts.Core.Entities;

namespace Accounts.Core.Ports.Driven
{
    public interface ILoginInfoRepository
    {
        Task AddLoginInfoAsync(LoginInformation loginInfo);
    }
}
