﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Application.Dtos
{
    public class VoterDto
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? NewEmail { get; set; }
        public string? CellPhone { get; set; }
        public DateTime? Birthday { get; set; }
        public bool? Status { get; set; }
    }
}