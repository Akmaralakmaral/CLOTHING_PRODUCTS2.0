using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYAPaymentController : Controller
    {
        private readonly AddDBContext _context;

        public KURSOVAYAPaymentController(AddDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? selectedCreditId)
        {
            // Устанавливаем выбранный ID кредита в ViewData для сохранения состояния
            ViewData["SelectedCreditId"] = selectedCreditId;

            // Вызываем хранимую процедуру для получения списка всех кредитов
            List<Credit> credits = await _context.Credits.FromSqlRaw("EXEC dbo.GetAllCredits").ToListAsync();

            // Создаем список SelectListItem для выпадающего списка кредитов
            var creditItems = credits.Select(c => new SelectListItem
            {
                Value = c.IdCredit.ToString(),
                Text = $"Сумма: {c.SumCredit}, Начало: {c.DataOfReceive.ToShortDateString()}, Окончание: {c.DataOfEnd.ToShortDateString()}"
            }).ToList();

            // Передаем список кредитов в ViewBag для представления
            ViewBag.CreditList = creditItems;

            // Если selectedCreditId не пустой, вызываем хранимую процедуру GetPaymentsByCreditId для получения списка выплат по выбранному кредиту
            if (selectedCreditId.HasValue && selectedCreditId.Value != 0)
            {
                // Вызываем хранимую процедуру для получения выплат и общих сумм
                var result = await _context.Payments
                    .FromSqlInterpolated($"EXEC dbo.GetPaymentsByCreditId {selectedCreditId}")
                    .ToListAsync();

                // Передаем результаты в представление
                return View(result);
            }

            // Если кредит не выбран, возвращаем представление с пустым списком выплат
            return View(new List<Payment>());
        }

        [HttpPost]
        public IActionResult Index(int selectedCreditId)
        {
            // Перенаправляем на GET-версию Index с выбранным ID кредита для загрузки списка выплат
            return RedirectToAction("Index", new { selectedCreditId });
        }

        public IActionResult Create(int? lastCreditId)
        {
            // Установка последнего выбранного кредита в ViewBag
            ViewBag.LastCreditId = lastCreditId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment, int? lastCreditId)
        {
            try
            {
                // Вызываем хранимую процедуру для создания новой выплаты
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    EXEC dbo.CreatePayment 
                        @paymentDate = {payment.PaymentDate}
                ");
               // int? selectedCreditId = ViewBag.SelectedCreditId as int?;

                // Редирект на Index с выбранным кредитом
                
                // Выводим сообщение об успешном создании выплаты
                TempData["Message"] = "Выплата успешно создана.";
                return RedirectToAction("Index", new { selectedCreditId = lastCreditId });
                // return RedirectToAction(nameof(Index), new { selectedCreditId });
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке, если что-то пошло не так
                TempData["Error"] = $"Ошибка при создании выплаты: {ex.Message}";
                ViewBag.LastCreditId = lastCreditId;
                return View(payment);

            }
            

           
        }

    }
}
