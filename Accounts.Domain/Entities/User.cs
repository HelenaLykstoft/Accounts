using Accounts.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }

    // Add foreign key for UserType (which you already have)
    public int UserTypeId { get; set; }

    // Add foreign key for ContactInfo
    public Guid ContactInfoId { get; set; } // Foreign Key to ContactInfo

    // Navigation properties
    public ContactInfo ContactInfo { get; set; } // Navigation property (optional but recommended for EF)
    public UserType UserType { get; set; } // Navigation property for UserType (optional)
    
}