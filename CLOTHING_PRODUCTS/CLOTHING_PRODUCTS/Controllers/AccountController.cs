using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AddDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(AddDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Поиск пользователя по логину и паролю в базе данных
                //var user = _context.Employees.FirstOrDefault(u => u.LoginName == model.LoginName && u.Password == model.Password);

                // Поиск пользователя по логину и паролю в базе данных
                var user = _context.Employees
                                .Include(e => e.PositionObject) // Включаем связанную сущность EmployeePosition
                                .ThenInclude(p => p.Role) // Затем включаем связанную сущность Role
                                .FirstOrDefault(u => u.LoginName == model.LoginName && u.Password == model.Password);

                if (user != null)
                {
                    // Получаем название роли
                     string userRole = user.PositionObject?.Role?.RoleName;
                     // Установка значения в сеансе
                     _httpContextAccessor.HttpContext.Session.SetString("UserRole", userRole);
 
                    return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            // Удаление значения из сеанса
 
            _httpContextAccessor.HttpContext.Session.Remove("UserRole");


            return RedirectToAction("Login", "Account");
        }
    }
}
