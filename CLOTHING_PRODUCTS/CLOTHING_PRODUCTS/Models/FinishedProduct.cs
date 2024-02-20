namespace CLOTHING_PRODUCTS.Models
{
    public class FinishedProduct
    {
        public int FinishedProductId { get; set; }
        public string Name { get; set; }
        public int MeasurementUnitId { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }

        public virtual MeasurementUnit MeasurementUnit { get; set; }
        public virtual ICollection<Ingredient> Ingredients { get; set; }
        public virtual ICollection<SaleProduct> SaleProducts { get; set; }
        public virtual ICollection<ProductManufacturing> ProductManufacturings { get; set; }
    }
}
