using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class RawMaterialController : Controller
    {
        private readonly AddDBContext _dbContext;
        public RawMaterialController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var rawMaterials = await _dbContext.RawMaterials
                .Include(e => e.MeasurementUnit)
                .ToListAsync();
            return View(rawMaterials);
        }

        public IActionResult Create()
        {
            ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RawMaterial rawMaterial)
        {
            // Check if a RawMaterial with the same name already exists
            if (_dbContext.RawMaterials.Any(rm => rm.Name == rawMaterial.Name))
            {
                ModelState.AddModelError("Name", "A RawMaterial with the same name already exists.");
                ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
                return View(rawMaterial);
            }

            // If validation passes, continue with the creation of the RawMaterial
            rawMaterial.Amount = 0.0;
            rawMaterial.Quantity = 0.0;
            _dbContext.RawMaterials.Add(rawMaterial);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var rawMaterial = _dbContext.RawMaterials
                .Include(e => e.MeasurementUnit)
                .FirstOrDefault(e => e.RawMaterialId == id);

            if (rawMaterial == null)
            {
                return NotFound();
            }

            ViewBag.MeasurementUnits = _dbContext.MeasurementUnits.ToList();
            return View(rawMaterial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RawMaterial editedRawMaterial)
        {
            var existingRawMaterial = await _dbContext.RawMaterials.FindAsync(id);

            if (existingRawMaterial == null)
            {
                return NotFound();
            }

            //// Загрузка существующих значений Quantity и Amount
            //editedRawMaterial.Quantity = existingRawMaterial.Quantity;
            //editedRawMaterial.Amount = existingRawMaterial.Amount;

            // Применение изменений и сохранение
            _dbContext.Entry(existingRawMaterial).CurrentValues.SetValues(editedRawMaterial);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rawMaterial = await _dbContext.RawMaterials.FindAsync(id);

            if (rawMaterial == null)
            {
                return NotFound();
            }

            _dbContext.RawMaterials.Remove(rawMaterial);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
