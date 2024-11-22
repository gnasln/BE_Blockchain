namespace Base_BE.Application.Dtos;

public class UserDto
{
    public string? FullName { get; set; }
    public bool? Gender { get; set; } = true;
    public string? Address { set; get; }
    public DateTime? Birthday { set; get; } = DateTime.MinValue;
    public new string? Email { get; set; }
    public string? CellPhone { get; set; }
    public string? Status { get; set; } = "Active";
    public bool? ChangePasswordFirstTime { get; set; } = false;
    public DateTime? CreateDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? TwoFaKey { get; set; }
    public bool? TwoFaEnable { get; set; }
    public string? EmailVerifyKey { get; set; }
    public string? NewEmail { get; set; }
    public string? IdentityCardImage { get; set; }
    public string? IdentityCardNumber { get; set; }
    public DateTime? IdentityCardDate { get; set; }
    public string? IdentityCardPlace { get; set; }
    public string? PublicKey { get; set; }
}