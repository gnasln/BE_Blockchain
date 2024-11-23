namespace Base_BE.Dtos;

public class CandidateDto
{
    public required string Id { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? NewEmail { get; set; }
    public string? Address { get; set; }
    public string? CellPhone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? ImageUrl { get; set; }
    public string? IdentityCardImage { get; set; }
    public int? TotalBallot { get; set; } = 0;
}