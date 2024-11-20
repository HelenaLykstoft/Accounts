using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Domain.Entities
{
    public class UserType
    {
        public int Id { get; set; }

        public string Type { get; set; }
        public ICollection<User> Users { get; set; }
    }
}