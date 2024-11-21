using Accounts.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }

    public int UserTypeId { get; set; }

    public Guid ContactInfoId { get; set; } 

    public ContactInfo ContactInfo { get; set; } 
    public UserType UserType { get; set; }
    
}