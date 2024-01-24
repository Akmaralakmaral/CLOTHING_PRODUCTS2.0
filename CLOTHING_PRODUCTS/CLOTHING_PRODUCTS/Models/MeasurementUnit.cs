namespace CLOTHING_PRODUCTS.Models
{
    public class MeasurementUnit
    {
        public int MeasurementUnitId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<RawMaterial> RawMaterials { get; set; }
        public virtual ICollection<FinishedProduct> FinishedProducts { get; set; }
    }
}
