using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AddDBContext _dbContext;

        public EmployeeController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var employees = await _dbContext.Employees
                .Include(e => e.PositionObject)
                .ToListAsync();

            return View(employees);
        }

        public IActionResult Create()
        {
            ViewBag.Positions = _dbContext.EmployeePositions.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {          
            _dbContext.Employees.Add(employee);
            employee.Position = "0";
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));          
           
        }

        public IActionResult Edit(int id)
        {
            var employee = _dbContext.Employees
                .Include(e => e.PositionObject)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Positions = _dbContext.EmployeePositions.ToList();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
           
            
            _dbContext.Entry(employee).State = EntityState.Modified;
            employee.Position = "0";
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _dbContext.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
