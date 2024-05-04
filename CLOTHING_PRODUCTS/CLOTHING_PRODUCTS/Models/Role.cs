namespace CLOTHING_PRODUCTS.Models
{
    public class Role
    {
        public int RoleId { get; set; } 
        public string RoleName{ get; set; } 
        public virtual ICollection<EmployeePosition> EmployeePositions { get; set; }
    }
}
