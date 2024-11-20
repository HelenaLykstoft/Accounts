using Accounts.Domain.Entities;

namespace Accounts.UnitTests.Entities;

public class TestUser:User
{
    public string Password { get; set; }
}