namespace Accounts.Core.Entities
{
    public class City
    {
        public int PostalCode { get; set; }

        public string Name { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}