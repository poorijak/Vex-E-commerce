using Microsoft.AspNetCore.Identity;

namespace Vex_E_commerce.Models
{
    public enum UserStatus
    {
        Active,
        Banned
    }
    public class Customer : IdentityUser
    {
        public UserStatus Status { get; set; } = UserStatus.Active;

        public string ProfilePictureUrl { get; set; }
    }
}
