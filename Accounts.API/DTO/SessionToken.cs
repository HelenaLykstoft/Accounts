namespace Accounts.API.DTO;

public class SessionToken
{
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime Expiry { get; set; }
}
