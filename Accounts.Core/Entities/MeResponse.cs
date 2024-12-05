namespace Accounts.Core.Entities
{
    public class MeResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public string Token { get; set; }
    }
}