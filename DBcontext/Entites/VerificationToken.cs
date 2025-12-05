using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.DBcontext.Entites
{
    [Table("VerificationTokens")]
    public class VerificationToken
    {
        [Key]
        public int TokenID { get; set; }

        public int CustomerID { get; set; }

        [Required]
        [StringLength(10)]
        public string TokenCode { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TokenType { get; set; } = null!; // "RESET_PASSWORD"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        // Relationship
        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
    }
}