using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class EmployeePositionController : Controller
    {
        private readonly AddDBContext _dbContext;

        public EmployeePositionController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var employeePositions = await _dbContext.EmployeePositions.Include(ep => ep.Role).ToListAsync();
            return View(employeePositions);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _dbContext.Roles.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeePosition newPosition)
        {
            // Check if an EmployeePosition with the same title already exists
            if (_dbContext.EmployeePositions.Any(ep => ep.Title == newPosition.Title))
            {
                ModelState.AddModelError("Title", "An Employee Position with the same title already exists.");
                return View(newPosition);
            }

            // If validation passes, continue with the creation of the EmployeePosition
            _dbContext.EmployeePositions.Add(newPosition);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


         public async Task<IActionResult> Edit(int id)
         {
            var positionToEdit = _dbContext.EmployeePositions
                .Include(ep =>ep.Role)
                .FirstOrDefault(ep => ep.EmployeePositionId == id);

            
            if (positionToEdit == null)
            {
                return NotFound(); // Если должность не найдена, возвращаем 404
            }
            ViewBag.Roles = _dbContext.Roles.ToList();

            return View(positionToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeePosition updatedPosition)
        {
            updatedPosition.EmployeePositionId = id;
            try
            {
                _dbContext.Entry(updatedPosition).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.EmployeePositions.Any(e => e.EmployeePositionId == id))
                {
                    return NotFound(); // Если должность не найдена, возвращаем 404
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
           
        }

        //public async Task<IActionResult> Delete(int id)
        //{
        //    var positionToDelete = await _dbContext.EmployeePositions.FindAsync(id);

        //    if (positionToDelete == null)
        //    {
        //        return NotFound();
        //    }

        //    _dbContext.EmployeePositions.Remove(positionToDelete);
        //    await _dbContext.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}


        public async Task<IActionResult> Delete(int id)
        {
            var positionToDelete = await _dbContext.EmployeePositions.FindAsync(id);

            if (positionToDelete == null)
            {
                return NotFound();
            }

            // Проверка наличия связанных записей в таблице Employees
            var hasRelatedEmployees = _dbContext.Employees.Any(e => e.PositionId == id);

            if (hasRelatedEmployees)
            {
                // Если есть связанные записи, выводим сообщение и не выполняем удаление
                TempData["ErrorMessage"] = "Невозможно удалить должность, так как она связана с сотрудниками.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.EmployeePositions.Remove(positionToDelete);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
