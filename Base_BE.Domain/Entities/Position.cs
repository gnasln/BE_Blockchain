using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_BE.Domain.Entities
{
    public class Position
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string PositionName { get; set; }
        public string PositionDescription { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }
}
