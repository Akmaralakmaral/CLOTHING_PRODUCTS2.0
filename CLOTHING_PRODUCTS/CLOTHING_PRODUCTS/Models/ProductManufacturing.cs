using System.ComponentModel.DataAnnotations.Schema;

namespace CLOTHING_PRODUCTS.Models
{
    public class ProductManufacturing
    {
        public int ID { get; set; }
        public int FinishedProductID { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }


        public string FinishedProductName { get; set; }
        public string EmployeeName { get; set; }


        public virtual FinishedProduct FinishedProduct { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
