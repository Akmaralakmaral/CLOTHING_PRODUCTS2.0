using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class FinishedProductController : Controller
    {
        private readonly AddDBContext _dbContext;
        public FinishedProductController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var finishedProducts = await _dbContext.FinishedProducts
                .Include(e => e.MeasurementUnit)
                .ToListAsync();
            return View(finishedProducts);
        }

        public IActionResult Create()
        {
            ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinishedProduct finishedProduct)
        {
            // Check if a FinishedProduct with the same name already exists
            if (_dbContext.FinishedProducts.Any(fp => fp.Name == finishedProduct.Name))
            {
                ModelState.AddModelError("Name", "A FinishedProduct with the same name already exists.");
                ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
                return View(finishedProduct);
            }

            // If validation passes, continue with the creation of the FinishedProduct
            finishedProduct.Amount = 0.0;
            finishedProduct.Quantity = 0.0;
            _dbContext.FinishedProducts.Add(finishedProduct);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var finishedProduct = _dbContext.FinishedProducts
                .Include(e => e.MeasurementUnit)
                .FirstOrDefault(e => e.FinishedProductId == id);

            if (finishedProduct == null)
            {
                return NotFound();
            }

            ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
            return View(finishedProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FinishedProduct editedFinishedProduct)
        {
            var existingFinishedProduct = await _dbContext.FinishedProducts.FindAsync(id);

            if (existingFinishedProduct == null)
            {
                return NotFound();
            }

            // Загрузка существующих значений Quantity и Amount
            editedFinishedProduct.Quantity = existingFinishedProduct.Quantity;
            editedFinishedProduct.Amount = existingFinishedProduct.Amount;

            // Применение изменений и сохранение
            _dbContext.Entry(existingFinishedProduct).CurrentValues.SetValues(editedFinishedProduct);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var finishedProduct = await _dbContext.FinishedProducts.FindAsync(id);

            if (finishedProduct == null)
            {
                return NotFound();
            }

            _dbContext.FinishedProducts.Remove(finishedProduct);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
