namespace Base_BE.Application.Dtos;

public class PositionResponse
{
    public Guid Id { get; set; }
    public string? PositionName { get; set; }
    public string? PositionDescription { get; set; } = string.Empty;
    public bool? Status { get; set; } = false;
}