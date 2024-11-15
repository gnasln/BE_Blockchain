using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_BE.Domain.Entities;

public class Vote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
  
    [MaxLength(255)]
    public required string VoteName { get; set; }
    public int MaxCandidateVote { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    
    [ForeignKey(nameof(Position))]
    public Guid PositionId { get; set; }
    public Position Position { get; set; }
    
    [MaxLength(50)]
    public string? Status { get; set; }
    
    [MaxLength(50)]
    public required string Tenure { get; set; }
    
    public DateTime StartDateTenure { get; set; }
    public DateTime EndDateTenure { get; set; }
    public string? ExtraData { get; set; }
    
    public ICollection<UserVote> UserVotes { get; set; }
}