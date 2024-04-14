namespace CLOTHING_PRODUCTS.Models
{
    public class EmployeePosition
    {
        public int EmployeePositionId { get; set; }
        public string Title { get; set;}

        public virtual ICollection<Employee> Employees { get;set; }
        public virtual Role Role { get; set; }

    }
}
