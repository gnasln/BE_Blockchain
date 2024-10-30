namespace Base_BE.Dtos;

public record UpdateUser
{
    public string? FullName { get; set; }
    public string? CellPhone { get; set; }
    public string? Address { get; set; }
}