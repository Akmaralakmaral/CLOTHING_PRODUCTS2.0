namespace CLOTHING_PRODUCTS.Models
{
    public class Salary
    {
        public int ID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int EmployeeID { get; set; }
        public int PurchaseCount { get; set; }
        public int ProductCount { get; set; }
        public int SalesCount { get; set; }
        public int CommonCount { get; set; }
        public double SalaryAmount { get; set; }
        public double Bonus { get; set; }
        public double General { get; set; }
        public int Issued { get; set; } // 0 - Not Issued, 1 - Issued

         public string EmployeeName { get; set; }
 
         public virtual Employee Employee { get; set; }
     }
}
