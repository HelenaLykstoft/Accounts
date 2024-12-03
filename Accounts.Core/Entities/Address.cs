using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Core.Entities
{
    public class Address
    {
        
        public Guid Id { get; set; }

        public int StreetNumber { get; set; }

        public string StreetName { get; set; }
        
        public int CityPostalCode { get; set; } 

        public City City { get; set; } 
        
        public ICollection<ContactInfo> ContactInfos { get; set; }
    }
}