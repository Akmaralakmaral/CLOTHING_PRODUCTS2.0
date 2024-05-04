using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYARoleController : Controller
    {
        private readonly AddDBContext _context;

        public KURSOVAYARoleController(AddDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // Вызов хранимой процедуры для получения всех кредитов
            List<Role> roles = _context.Roles.FromSqlRaw("EXEC dbo.GetAllRoles").ToList();

            // Передача списка кредитов в представление для отображения
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var role = new Role(); 
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            
            try
            {
                // Вызов хранимой процедуры для создания роли
                await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.CreateRole {role.RoleName}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(role);
            
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Получение роли по идентификатору
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Role role)
        {
           
            try
            {
                // Вызов хранимой процедуры для обновления роли
                await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.UpdateRole {role.RoleId}, {role.RoleName}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(role);
            }
                        
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Вызов хранимой процедуры для удаления роли
                await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.DeleteRole {id}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

    }
}
