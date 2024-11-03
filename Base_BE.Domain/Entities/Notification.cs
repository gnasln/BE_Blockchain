using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Domain.Entities
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public string FiredUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(ApplicationUser1))]
        public string ReceiveUserId { get; set; }
        public ApplicationUser ApplicationUser1 { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }

        public bool IsRead { get; set; }

        public bool IsTrash { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedAt { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Content { get; set; }

        [MaxLength(1000)]
        public string ExtraInfo { get; set; }
    }
}
