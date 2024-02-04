namespace CLOTHING_PRODUCTS.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public int FinishedProductId { get; set; }
        public int RawMaterialId { get; set; }
        public double Quantity { get; set; }

        public virtual FinishedProduct FinishedProduct { get; set; }
        public virtual RawMaterial RawMaterial { get; set; }
    }
}
