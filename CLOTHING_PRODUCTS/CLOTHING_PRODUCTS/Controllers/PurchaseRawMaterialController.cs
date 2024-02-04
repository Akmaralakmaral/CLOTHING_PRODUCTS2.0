using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class PurchaseRawMaterialController : Controller
    {
        private readonly AddDBContext _dbContext;

        public PurchaseRawMaterialController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var purchaseRawMaterials = await _dbContext.PurchaseRawMaterials
                .Include(e => e.RawMaterial)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(purchaseRawMaterials);
        }

        public IActionResult Create()
        {
            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
            ViewBag.Employees = _dbContext.Employees.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RawMaterialID,Quantity,Amount,Date,EmployeeID")] PurchaseRawMaterial purchaseRawMaterial)
        {           
            // Проверяем, хватит ли бюджета для закупки данного сырья на указанную сумму
            var budget = await _dbContext.Budgets.FirstOrDefaultAsync();

            if (budget != null && budget.BudgetAmount >= purchaseRawMaterial.Amount)
            {
                // Уменьшаем бюджет на сумму закупки
                budget.BudgetAmount -= purchaseRawMaterial.Amount;

                // Увеличиваем склад сырья на закупаемое количество
                var rawMaterial = await _dbContext.RawMaterials.FindAsync(purchaseRawMaterial.RawMaterialID);
                if (rawMaterial != null)
                {
                    rawMaterial.Quantity += purchaseRawMaterial.Quantity;
                    rawMaterial.Amount += purchaseRawMaterial.Amount;
                }

                // Добавляем запись о закупке сырья
                _dbContext.Add(purchaseRawMaterial);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                ViewBag.Employees = _dbContext.Employees.ToList();
                // Если бюджет не позволяет купить сырье на данную сумму, возвращаем ошибку
                ModelState.AddModelError(string.Empty, "Недостаточно средств в бюджете для закупки сырья");
                return View(new PurchaseRawMaterial()); // Возвращаем представление с пустой моделью


                //// Если бюджет не позволяет купить сырье на данную сумму, возвращаем ошибку
                //ModelState.AddModelError(string.Empty, "Недостаточно средств в бюджете для закупки сырья");
                //return View(purchaseRawMaterial);
            }          
        }
    }
}
