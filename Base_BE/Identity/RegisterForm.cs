using System.ComponentModel.DataAnnotations;

namespace Base_BE.Identity
{
    public record RegisterForm
    {
        public string? FullName { get; set; }
        public string? IdentityCardNumber { get; set; }
        public DateTime? IdentityCardDate { get; set; }
        public string? IdentityCardPlace { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public bool? Gender { get; set; }
        public string? CellPhone { get; set; }
        public string? ImageUrl { get; set; }
		public string? UrlIdentityCardImage { get; set; }
		public bool? IsAdmin { get; set; }

	}
}
