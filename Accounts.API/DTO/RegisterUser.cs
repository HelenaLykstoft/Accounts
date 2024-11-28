using System.ComponentModel;
using Accounts.Domain.Entities;

namespace Accounts.API.DTO
{
    public class RegisterUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        [ReadOnly(true)]
        public int UserTypeId { get; set; } 

        // Contact Information
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Address Information
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
        public int PostalCode { get; set; } 
        public string City { get; set; } 
    }
}

