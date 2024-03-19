namespace CLOTHING_PRODUCTS.ViewModels
{
    public class IngredientViewModel
    {
        public int IngredientId { get; set; }
        public int FinishedProductId { get; set; }
        public int RawMaterialId { get; set; }
        public double Quantity { get; set; }
        public string RawMaterialName { get; set; } // Название сырья
    }
}
