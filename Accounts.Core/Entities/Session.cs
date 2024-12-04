namespace Accounts.Core.Entities
{
    public class Session
    {
        public Guid UserId { get; set; }
        public DateTime Expiry { get; set; }
    }
}
