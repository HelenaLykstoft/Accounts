using Accounts.Core.Ports.Driven;

namespace Accounts.Infrastructure.Persistence
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
