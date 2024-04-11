using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class BudgetController : Controller
    {
        private readonly AddDBContext _dbContext;

        public BudgetController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

       public async Task<IActionResult> Index(int? year, int? month)
       {
            var budgets = await _dbContext.Budgets.ToListAsync();
            if (year != null && month != null)
            {
                
                ViewBag.SelectedYear = year;
                ViewBag.SelectedMonth = month;

            }
            return View(budgets);
       }


        public async Task<IActionResult> Edit(int id, int? year, int? month)
        {
            var budgetToEdit = await _dbContext.Budgets.FindAsync(id);

            if(budgetToEdit == null) 
            {
                return NotFound();
            }
            ViewBag.SelectedYear = year;
            ViewBag.SelectedMonth = month;

            return View(budgetToEdit);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget updateBudget, int? selectedYear, int? selectedMonth)
        {
            updateBudget.BudgetId = id;
            try
            {
                _dbContext.Entry(updateBudget).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) 
            {
                if(!_dbContext.Budgets.Any(e => e.BudgetId == id))
                {
                    return NotFound();

                }
                else
                {
                    throw;
                }
            }
            // Добавляем выбранный год и месяц в ViewBag
            if (selectedYear != null && selectedMonth != null)
            {
                ViewBag.SelectedYear = selectedYear;
                ViewBag.SelectedMonth = selectedMonth;
            }
            // Перенаправляем пользователя на страницу Index с выбранным годом и месяцем
            return RedirectToAction(nameof(Index), new { year = selectedYear, month = selectedMonth });

        }
    }
}
