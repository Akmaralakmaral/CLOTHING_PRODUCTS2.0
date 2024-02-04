using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Context
{
    public class AddDBContext : IdentityDbContext
    {
        public AddDBContext(DbContextOptions<AddDBContext> options) : base(options) 
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<FinishedProduct> FinishedProducts { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<MeasurementUnit> MeasurementUnits { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<PurchaseRawMaterial> PurchaseRawMaterials { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-40QVERS;Initial Catalog=CLOTHING_PRODUCTS2.0;User ID=sa;Password=2004;TrustServerCertificate=true");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.PositionObject)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RawMaterial>()
               .HasOne(rm => rm.MeasurementUnit)
               .WithMany(mu => mu.RawMaterials)
               .HasForeignKey(rm => rm.MeasurementUnitId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinishedProduct>()
                .HasOne(fp => fp.MeasurementUnit)
                .WithMany(mu => mu.FinishedProducts)
                .HasForeignKey(fp => fp.MeasurementUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Ingredient>()
            //.HasKey(i => new { i.FinishedProductId, i.RawMaterialId });

            modelBuilder.Entity<Ingredient>()
                .HasOne(i => i.FinishedProduct)
                .WithMany(fp => fp.Ingredients)
                .HasForeignKey(i => i.FinishedProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ingredient>()
                .HasOne(j => j.RawMaterial)
                .WithMany(fp => fp.Ingredients)
                .HasForeignKey(j => j.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseRawMaterial>()
                .HasOne(prm => prm.RawMaterial)
                .WithMany(rm => rm.PurchaseRawMaterials)
                .HasForeignKey(prm => prm.RawMaterialID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseRawMaterial>()
                .HasOne(prm => prm.Employee)
                .WithMany(e => e.PurchaseRawMaterials)
                .HasForeignKey(prm => prm.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
