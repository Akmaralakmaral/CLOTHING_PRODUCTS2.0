using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
            ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName");
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("FinishedProductID, Quantity, Amount, Date, EmployeeID")] SaleProduct saleProduct)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _dbContext.Add(saleProduct);
        //        await _dbContext.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name", saleProduct.FinishedProductID);
        //    ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", saleProduct.EmployeeID);
        //    return View(saleProduct);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FinishedProductID, Quantity, Date, EmployeeID")] SaleProduct saleProduct)
        {
          
            
            var finishedProduct = await _dbContext.FinishedProducts.FindAsync(saleProduct.FinishedProductID);

            
            if (finishedProduct.Quantity < saleProduct.Quantity)
            {
                ViewData["InsufficientQuantityMessage"] = "Недостаточно товара на складе.";
                ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
                ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", saleProduct.EmployeeID);
                return View(saleProduct);
            }

            
            finishedProduct.Quantity -= saleProduct.Quantity;

            var budget = await _dbContext.Budgets.FirstOrDefaultAsync();

            
            var saleAmount = saleProduct.Quantity * (finishedProduct.Amount / finishedProduct.Quantity) *
                                ((budget.Percent) / 100);

            
            budget.BudgetAmount += saleAmount;
            saleProduct.Amount = saleAmount;
            finishedProduct.Amount -= saleProduct.Amount;
            
            _dbContext.Add(saleProduct);

            
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
          
        }




    }
}
