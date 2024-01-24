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
            var employeePositions = await _dbContext.EmployeePositions.ToListAsync();
            return View(employeePositions);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeePosition newPosition)
        {
           
            
            _dbContext.EmployeePositions.Add(newPosition); // Добавление новой должности в контекст
            await _dbContext.SaveChangesAsync(); // Сохранение изменений в базе данных
            return RedirectToAction(nameof(Index)); // Перенаправление на метод Index
            
        }


        public async Task<IActionResult> Edit(int id)
        {
            var positionToEdit = await _dbContext.EmployeePositions.FindAsync(id);

            if (positionToEdit == null)
            {
                return NotFound(); // Если должность не найдена, возвращаем 404
            }

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
