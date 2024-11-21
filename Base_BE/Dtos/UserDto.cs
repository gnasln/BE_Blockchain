namespace Base_BE.Dtos
{
    public class UserDto
    {
        public required string Id { get; set; }
        public string? Fullname { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? NewEmail { get; set; }
        public string? IdentityCardNumber { get; set; }
        public DateTime? IdentityCardDate { get; set; }
        public string? IdentityCardPlace { get; set; }
        public bool? Gender { get; set; }
        public string? CellPhone { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? status { get; set; }
        public string? Role { get; set; }
        public string? ImageUrl { get; set; }
        public string? IdentityCardImage { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
