using Base_BE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Application.Dtos
{
    public class UserVoteDto
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public Guid VoteId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Role { get; set; }

        public string BallotTransaction { get; set; }

        public string BallotAddress { get; set; }

        public bool Status { get; set; }
    }
}
