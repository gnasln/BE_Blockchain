namespace Base_BE.Identity;

public record SendOTPRequest
{
    public required string Email { get; set; }
}