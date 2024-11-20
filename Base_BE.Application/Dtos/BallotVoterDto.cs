using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Application.Dtos
{
    public class BallotVoterDto
    {
        public Guid Id { get; set; }
        public List<Guid> CandidateIds { get; set; }
        public Guid VoterId { get; set; }
        public DateTime VotedTime { get; set; } = DateTime.Now;
        public string? Address { get; set; }
    }
}
