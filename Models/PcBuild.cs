using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class PcBuild
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string ComponentsJson { get; set; } 

        public decimal TotalPrice { get; set; }

        public DateTime LastUpdated { get; set; }

    }
}
