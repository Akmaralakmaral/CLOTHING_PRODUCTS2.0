using System.ComponentModel.DataAnnotations;

namespace CLOTHING_PRODUCTS.Models
{
    public class Credit
    {
        [Key]
        public int IdCredit { get; set; }
        public double SumCredit { get; set; }
        public DateTime DataOfReceive { get; set; }
        public DateTime DataOfEnd { get; set; }
        public int Months { get; set; }
        public double Perst { get; set; }
        public double Penny { get; set; }
    }
}
