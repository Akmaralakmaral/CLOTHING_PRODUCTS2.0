using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class SaleProductController : Controller
    {
        private readonly AddDBContext _dbContext;

        public SaleProductController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var saleProducts = await _dbContext.SaleProducts
                .Include(e => e.FinishedProduct)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(saleProducts);
        }

        public IActionResult Create()
        {
            ViewBag.FinishedProducts = _dbContext.FinishedProducts.ToList();
            ViewBag.Employees = _dbContext.Employees.ToList();
            return View();
        }


    }
}
