namespace Base_BE.Identity;

public record SendOTPRequest
{
    public string? Email { get; set; }
}