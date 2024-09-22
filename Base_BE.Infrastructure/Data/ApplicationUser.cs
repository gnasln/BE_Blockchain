using Microsoft.AspNetCore.Identity;

namespace Base_BE.Infrastructure.Data
{
    public class ApplicationUser : IdentityUser
    {
        
        public string? FullName { get; set; }
        public string? DisplayName { get; set; }
        public string? Address { set; get; }
        public string? Status { get; set; } = "0";
        public string? OTP { set; get; }
        public DateTime? ActivationDate { get; set; } = DateTime.MinValue;
        public DateTime? Birthday { set; get; } = DateTime.MinValue;
        public DateTime? Expires_at { get; set; }
        public DateTime? Updated_at { get; set; }

    }
}
