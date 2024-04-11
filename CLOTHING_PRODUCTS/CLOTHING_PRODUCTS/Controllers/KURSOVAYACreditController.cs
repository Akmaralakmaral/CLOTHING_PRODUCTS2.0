using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYACreditController : Controller
    {
        private readonly AddDBContext _context;

        public KURSOVAYACreditController(AddDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Вызов хранимой процедуры для получения всех кредитов
            List<Credit> credits = _context.Credits.FromSqlRaw("EXEC dbo.GetAllCredits").ToList();

            // Передача списка кредитов в представление для отображения
            return View(credits);
        }

        public IActionResult Create()
        {
            // Установка сегодняшней даты по умолчанию для DataOfReceive
            var defaultDate = DateTime.Today;

            // Создаем новый объект Credit для передачи в представление
            var credit = new Credit
            {
                DataOfReceive = defaultDate
            };

            return View(credit);
        }

        [HttpPost]
        public IActionResult Create(Credit credit)
        {
            try
            {
                // Вызываем хранимую процедуру для создания кредита
                _context.Database.ExecuteSqlInterpolated($"EXEC dbo.CreateCredit {credit.SumCredit}, {credit.DataOfReceive}, {credit.Months}, {credit.Perst}, {credit.Penny}");

                // Редирект на страницу списка кредитов после успешного создания
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Обработка ошибки создания кредита
                ModelState.AddModelError(string.Empty, "Ошибка при создании кредита: " + ex.Message);
                // Если модель недопустима, возвращаем представление с моделью для исправления ошибок
                return View(credit);
            }
            

            
        }

    }
}
