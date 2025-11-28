using System.ComponentModel.DataAnnotations;

namespace Vex_E_commerce.Models
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        public string Name { get; set; } 

        [Required, MaxLength(255)]
        public string Phone { get; set; } 

        [Required, MaxLength(255)]
        public string AddressDetail { get; set; } 

        [Required, MaxLength(255)]
        public string Province { get; set; } 

        [Required, MaxLength(255)]
        public string PostalCode { get; set; } 

        [MaxLength(255)]
        public string? Note { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public Order? Order { get; set; }
    }
}
