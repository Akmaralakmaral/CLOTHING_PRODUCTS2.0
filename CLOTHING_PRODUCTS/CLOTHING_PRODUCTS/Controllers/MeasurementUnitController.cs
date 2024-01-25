using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class MeasurementUnitController : Controller
    {
        private readonly AddDBContext _dbContext;

        public MeasurementUnitController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var measurementUnits = await _dbContext.MeasurementUnits.ToListAsync();
            return View(measurementUnits);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MeasurementUnit measurementUnit)
        {
            // Check if a MeasurementUnit with the same name already exists
            if (_dbContext.MeasurementUnits.Any(mu => mu.Name == measurementUnit.Name))
            {
                ModelState.AddModelError("Name", "A MeasurementUnit with the same name already exists.");
                return View(measurementUnit);
            }

            // If validation passes, continue with the creation of the MeasurementUnit
            _dbContext.MeasurementUnits.Add(measurementUnit);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var measurementUnit = await _dbContext.MeasurementUnits.FindAsync(id);

            if (measurementUnit == null)
            {
                return NotFound();
            }

            return View(measurementUnit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MeasurementUnit measurementUnit)
        {
            if (id != measurementUnit.MeasurementUnitId)
            {
                return NotFound();
            }

           
            try
            {
                _dbContext.Update(measurementUnit);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeasurementUnitExists(measurementUnit.MeasurementUnitId))
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

        private bool MeasurementUnitExists(int id)
        {
            return _dbContext.MeasurementUnits.Any(e => e.MeasurementUnitId == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var measurementUnitsToDelete = await _dbContext.MeasurementUnits.FindAsync(id);

            if (measurementUnitsToDelete == null)
            {
                return NotFound();
            }

            // Проверка наличия связанных записей в таблице Employees
            var hasRelatedRawMaterials = _dbContext.RawMaterials.Any(e => e.MeasurementUnitId == id);
            var hasRelatedFinishedProducts = _dbContext.FinishedProducts.Any(e => e.MeasurementUnitId == id);
            if (hasRelatedRawMaterials)
            {
                // Если есть связанные записи, выводим сообщение и не выполняем удаление
                TempData["ErrorMessage1"] = "Невозможно удалить единицу измерения, так как она связана с таблицей RawMaterial.";
                return RedirectToAction(nameof(Index));
            }
            if (hasRelatedFinishedProducts)
            {
                // Если есть связанные записи, выводим сообщение и не выполняем удаление
                TempData["ErrorMessage2"] = "Невозможно удалить единицу измерения, так как она связана с таблицей FinishedProduct.";
                return RedirectToAction(nameof(Index));
            }
            _dbContext.MeasurementUnits.Remove(measurementUnitsToDelete);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
