using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }     // e.g., COD, BankTransfer, Installment
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;

    }
}
