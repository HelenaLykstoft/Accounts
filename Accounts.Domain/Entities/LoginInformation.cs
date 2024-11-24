using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Domain.Entities
{
    public class LoginInformation
    {
        public string Username { get; set; }

        public string Password { get; set; }
        public User User { get; set; }
    }
}