namespace PcSaler.DBcontext.Entites
{
    public class CustomPCDetail
    {
        public int CustomPCDetailID { get; set; }
        public int CustomPCID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }

        public CustomPC CustomPC { get; set; }
        public Product Product { get; set; }

    }
}
