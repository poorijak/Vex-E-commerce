using Microsoft.AspNetCore.Identity;

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

    }
}
