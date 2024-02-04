namespace CLOTHING_PRODUCTS.Models
{
    public class PurchaseRawMaterial
    {
        public int ID { get; set; }
        public int RawMaterialID { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }

        public virtual RawMaterial RawMaterial { get; set; }
        public virtual Employee Employee { get; set; }


        
    }
}
