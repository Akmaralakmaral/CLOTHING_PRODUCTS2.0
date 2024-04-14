namespace CLOTHING_PRODUCTS.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName{ get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }

        // Навигационное свойство для связи с должностями (EmployeePositions)
        public virtual ICollection<EmployeePosition> EmployeePositions { get; set; }
    }
}
