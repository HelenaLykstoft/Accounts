namespace Accounts.Core.Ports.Driven
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
    }
}