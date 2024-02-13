namespace CLOTHING_PRODUCTS.Models
{
    public class SaleProduct
    {
        public int ID { get; set; }
        public int FinishedProductID { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }

        public virtual FinishedProduct FinishedProduct { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
