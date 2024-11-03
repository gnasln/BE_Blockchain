using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Domain.Entities
{
    public class BallotVoter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }
        public Guid VoterId { get; set; }

        public DateTime VotedTime { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }
    }
}
