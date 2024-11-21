namespace Base_BE.Dtos;

public class CandidateDto
{
    public required string Id { get; set; }
    public string? FullName { get; set; }
    public int? TotalBallot { get; set; } = 0;
}