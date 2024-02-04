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

       public async Task<IActionResult> Index()
       {
            var budgets = await _dbContext.Budgets.ToListAsync();     
            return View(budgets);
       }


        public async Task<IActionResult> Edit(int id)
        {
            var budgetToEdit = await _dbContext.Budgets.FindAsync(id);

            if(budgetToEdit == null) 
            {
                return NotFound();
            }


            return View(budgetToEdit);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget updateBudget)
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
            return RedirectToAction(nameof(Index));
        }
    }
}
