using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYASalaryController : Controller
    {
        private readonly AddDBContext _context;

        public KURSOVAYASalaryController(AddDBContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? year, int? month, bool? fromEditGeneral)
        {
            // Если год и месяц не были переданы, формируем списки годов и месяцев для выпадающих списков
            List<int> years = Enumerable.Range(2020, 11).ToList();
            // Получаем список месяцев с помощью хранимой процедуры GetMonthsList
            List<Month> months = _context.Months.FromSqlRaw("EXEC [dbo].[GetMonthsList]").ToList();
            ViewBag.Years = years;
            ViewBag.Months = months;

            if (year != null && month != null)
            {
                // Вычисляем общую сумму General для выбранного года и месяца
                var totalGeneral = _context.Salaries
                    .Where(s => s.Year == year && s.Month == month)
                    .Sum(s => s.General);

                ViewBag.TotalGeneral = totalGeneral;
                if (fromEditGeneral.GetValueOrDefault())
                {
                    var salaries = _context.Salaries.FromSqlRaw("EXEC [dbo].[GetSalaries] @Year, @Month",
                        new Microsoft.Data.SqlClient.SqlParameter("@Year", year),
                        new Microsoft.Data.SqlClient.SqlParameter("@Month", month)).ToList();
                    fromEditGeneral = null;
                    return View(salaries);
                }
                else
                {
                    _context.Database.ExecuteSqlRaw("EXEC [dbo].[CreateSalary] @Year, @Month",
                        new Microsoft.Data.SqlClient.SqlParameter("@Year", year),
                        new Microsoft.Data.SqlClient.SqlParameter("@Month", month));

                    var salaries = _context.Salaries.FromSqlRaw("EXEC [dbo].[GetSalaries] @Year, @Month",
                        new Microsoft.Data.SqlClient.SqlParameter("@Year", year),
                        new Microsoft.Data.SqlClient.SqlParameter("@Month", month)).ToList();
                    return View(salaries);
                }
            }

            return View(new List<Salary>());
        }


        // GET: KURSOVAYASalary/EditGeneral/5
        public IActionResult EditGeneral(int? id)
        {
            var salary = _context.Salaries
                     .Where(s => s.ID == id)
                     .Select(s => new Salary { ID = s.ID, General = s.General, Year = s.Year, Month = s.Month })
                     .FirstOrDefault();
            // Получаем значения года и месяца из запроса
            int year = salary.Year;
            int month = salary.Month;

            ViewBag.Year = year;
            ViewBag.Month = month;

            return View(salary);
        }

        // POST: KURSOVAYASalary/EditGeneral/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditGeneral(int id, [Bind("ID,General")] Salary salary, int year, int month)
        {
            _context.Database.ExecuteSqlInterpolated($"EXEC [dbo].[UpdateGeneral] {salary.ID}, {salary.General}");

            // Устанавливаем состояние сущности как измененное только для поля General
            _context.Entry(salary).Property("General").IsModified = true;
            _context.SaveChanges();

            // Передача года и месяца при перенаправлении
            return RedirectToAction(nameof(Index), new { year = year, month = month, fromEditGeneral = true });
        }

    }
}
    
