using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_BE.Domain.Entities;

public class UserVote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(ApplicationUser))]
    public string UserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    [ForeignKey(nameof(Vote))]
    public Guid VoteId { get; set; }
    public Vote Vote { get; set; }

    public DateTime CreatedDate { get; set; }

    [MaxLength(50)]
    public string Role { get; set; }

    [MaxLength(255)]
    public string BallotTransaction { get; set; }

    [MaxLength(255)]
    public string BallotAddress { get; set; }

    public bool Status { get; set; }
    
}