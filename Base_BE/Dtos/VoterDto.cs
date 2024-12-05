namespace Base_BE.Dtos;

public class VoterDto
{
    public string? Id { get; set; }
    public string? Fullname { get; set; }
    public string? Email { get; set; }
    public string? NewEmail { get; set; }
    public string? Address { get; set; }
    public string? CellPhone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? ImageUrl { get; set; }
    public bool? Status { get; set; }
}