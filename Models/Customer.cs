using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Vex_E_commerce.Models
{
    public enum UserStatus
    {
        Active,
        Banned
    }
    public enum UserRole
    {
        Customer,
        Admin
    }
    public class Customer : IdentityUser
    {
        public UserStatus Status { get; set; } = UserStatus.Active;

        public UserRole Role { get; set; } = UserRole.Customer;

        public string ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MinLength(3, ErrorMessage = ("MinLength 3 char"))]
        public string Name { get; set; }

        public ICollection<Address> addresses { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<ProductWishlist> WishlistsItems { get; set; } = new List<ProductWishlist>();
    }


    public class CustomerVm
    {

        public CustomerFormDto Form { get; set; } = new();


        public Customer? User { get; set; } = new Customer();


        public List<Order> Orders { get; set; } = new();

    }

    public class CustomerFormDto
    {
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }

        public string Id { get; set; }
    }
}
