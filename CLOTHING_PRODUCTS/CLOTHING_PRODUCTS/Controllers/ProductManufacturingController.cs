using CLOTHING_PRODUCTS.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class ProductManufacturingController : Controller
    {
        private readonly AddDBContext _dbContext;

        public ProductManufacturingController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var productManufacturings = await _dbContext.ProductManufacturings
                .Include(e => e.FinishedProduct)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(productManufacturings);
        }

        public IActionResult Create()
        {
            ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
            ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName");
            return View();
        }
    }
}
