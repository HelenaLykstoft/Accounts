using Accounts.Domain.Entities;

namespace Accounts.API.DTO
{
    public class RegisterUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        
        public string Password { get; set; }
        public int UserTypeId { get; set; }  // References user_type table

        // Contact Information
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Address Information
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
        public int PostalCode { get; set; } // References city table
        public string City { get; set; } // Name of the city for creation (in case the city is new)
    }
}