using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class SalaryController : Controller
    {
        private readonly AddDBContext _context;

        public SalaryController(AddDBContext context)
        {
            _context = context;
        }
        //public async Task<IActionResult> Index()
        //{
        //    // Получаем все записи о зарплате из базы данных
        //    var salaries = await _context.Salaries
        //        .Include(s => s.Employee) // Подгружаем связанные данные о сотруднике
        //        .ToListAsync();

        //    return View(salaries);
        //}

        //public async Task<IActionResult> Index(int? selectedYear, int? selectedMonth)
        //{
        //    var salariesQuery = _context.Salaries
        //        .Include(s => s.Employee);

        //    // Если выбран год и месяц, фильтруем по ним
        //    if (selectedYear.HasValue && selectedMonth.HasValue)
        //    {
        //        salariesQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Salary, Employee>)salariesQuery
        //            .Where(s => s.Year == selectedYear && s.Month == selectedMonth);
        //    }

        //    var salaries = await salariesQuery.ToListAsync();

        //    // Заполняем ViewBag для выпадающих списков года и месяца
        //    ViewBag.SelectedYear = selectedYear;
        //    ViewBag.SelectedMonth = selectedMonth;
        //    ViewBag.Years = Enumerable.Range(2020, 10).Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() });
        //    ViewBag.Months = GetMonthNames().Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString() });

        //    return View(salaries);
        //}

        public IActionResult Index(List<Salary> filteredSalaries)
        {
            // Заполняем ViewBag для выпадающих списков года и месяца
            ViewBag.Years = Enumerable.Range(2020, 10).Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() });
            ViewBag.Months = GetMonthNames().Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString() });

            // Отобразить все зарплаты, если список пустой
            if (filteredSalaries == null || filteredSalaries.Count == 0)
            {
                var allSalaries = _context.Salaries.Include(s => s.Employee).ToList();
                return View(allSalaries);
            }

            return View(filteredSalaries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FilterSalaries(int? selectedYear, int? selectedMonth)
        {
            var salariesQuery = _context.Salaries;

            // Если выбран год и месяц, фильтруем по ним
            if (selectedYear.HasValue && selectedMonth.HasValue)
            {
                salariesQuery = (DbSet<Salary>)salariesQuery
                    .Where(s => s.Year == selectedYear && s.Month == selectedMonth)
                    .Include(s => s.Employee);
            }

            var filteredSalaries = salariesQuery.ToList();

            // Заполняем ViewBag для выпадающих списков года и месяца
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.Years = Enumerable.Range(2020, 10).Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() });
            ViewBag.Months = GetMonthNames().Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString() });

            // Возвращаем View с отфильтрованными зарплатами
            return RedirectToAction(nameof(Index), new { filteredSalaries });
        }


        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_context.Employees.ToList(), "EmployeeId", "FullName");
            ViewBag.Months = GetMonthNames().Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString() });
            ViewBag.Years = Enumerable.Range(2020, 10).Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() });
            return View();
        }
        private IDictionary<int, string> GetMonthNames()
        {
            var monthNames = new Dictionary<int, string>();
            for (int month = 1; month <= 12; month++)
            {
                monthNames.Add(month, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            }
            return monthNames;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Salary salary)
        {
            // Рассчитываем значения ProductCount, SalesCount и PurchaseCount на основе выбранных данных
            salary.ProductCount = _context.ProductManufacturings
                                            .Where(pm => pm.EmployeeID == salary.EmployeeID &&
                                                         pm.Date.Year == salary.Year &&
                                                         pm.Date.Month == salary.Month)
                                            .Count();

            salary.SalesCount = _context.SaleProducts
                                            .Where(sp => sp.EmployeeID == salary.EmployeeID &&
                                                         sp.Date.Year == salary.Year &&
                                                         sp.Date.Month == salary.Month)
                                            .Count();

            salary.PurchaseCount = _context.PurchaseRawMaterials
                                            .Where(prm => prm.EmployeeID == salary.EmployeeID &&
                                                          prm.Date.Year == salary.Year &&
                                                          prm.Date.Month == salary.Month)
                                            .Count();

            // Вычисляем общее количество (CommonCount)
            salary.CommonCount = salary.ProductCount + salary.SalesCount + salary.PurchaseCount;

            // Получаем информацию о бюджете
            var budget = _context.Budgets.FirstOrDefault(); // Предп2оложим, что у вас есть только одна запись бюджета

            // Устанавливаем значение зарплаты (SalaryAmount)
            salary.SalaryAmount = _context.Employees
                                            .Where(e => e.EmployeeId == salary.EmployeeID)
                                            .Select(e => e.Salary)
                                            .FirstOrDefault();

            // Устанавливаем значение бонуса (Bonus) из бюджета
            salary.Bonus = budget.Bonus;

            double bonus2 = (salary.CommonCount * salary.Bonus) / 100;

            // Вычисляем значение General
            salary.General = (bonus2 + 1) * salary.SalaryAmount;

            // Устанавливаем значение Issued
            salary.Issued = 0;

            // Создаем новую зарплату сотрудника
            _context.Add(salary);
            _context.SaveChanges();


            return RedirectToAction(nameof(Index));
        }

    }
}
