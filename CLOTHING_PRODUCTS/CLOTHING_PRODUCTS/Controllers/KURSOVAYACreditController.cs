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
    }
}
