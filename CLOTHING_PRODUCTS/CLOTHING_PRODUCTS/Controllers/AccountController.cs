using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AddDBContext _context;

        public AccountController(AddDBContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            var model = new LoginViewModel();
           
            return View(model);
        }

        // Действие для обработки входа пользователя
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Поиск пользователя по логину и паролю в базе данных
                var user = _context.Roles.FirstOrDefault(u => u.LoginName == model.LoginName && u.Password == model.Password);

                if (user != null)
                {
                    // Успешная аутентификация - установка метки в сессии или иного механизма аутентификации
                    //HttpContext.Session.SetString("LoginName", user.LoginName);

                    return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            
            return View(model);
        }

        // Действие для выхода пользователя (необходимо реализовать)
        public IActionResult Logout()
        {
            // Очистка сессии или других данных аутентификации

            return RedirectToAction("Index", "Home");
        }
    }
}
