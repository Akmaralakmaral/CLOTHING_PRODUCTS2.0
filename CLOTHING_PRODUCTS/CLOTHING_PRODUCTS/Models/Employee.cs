namespace CLOTHING_PRODUCTS.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public double Salary { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int PositionId { get; set; }

        public virtual EmployeePosition PositionObject { get; set; }
        public virtual ICollection<PurchaseRawMaterial> PurchaseRawMaterials { get; set; }
        public virtual ICollection<SaleProduct> SaleProducts { get; set; }

    }
}
