using System.ComponentModel.DataAnnotations;

namespace CLOTHING_PRODUCTS.Models
{
    public class Payment
    {
        [Key]
        public int IdPayment { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDeadline { get; set; }
        public double MainSum { get; set; }
        public double PerSum { get; set; }
        public double ComSum { get; set; }
        public double Missed { get; set; }
        public double PennySum { get; set; }
        public double Total { get; set; }
        public double Remains { get; set; }
    }
}
