using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class SalaryTwoController : Controller
    {
        private readonly AddDBContext _context;

        public SalaryTwoController(AddDBContext context)
        {
            _context = context;
        }

        //public async Task<IActionResult> Index(int? selectedYear, int? selectedMonth)
        //{
        //    int currentYear = DateTime.Now.Year;
        //    int currentMonth = DateTime.Now.Month;

        //    List<int> years = await _context.Salaries.Select(s => s.Year).Distinct().ToListAsync();

        //    // Создаем список месяцев с использованием перечисления System.Globalization.DateTimeFormatInfo
        //    List<string> months = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();

        //    selectedYear ??= currentYear;
        //    selectedMonth ??= currentMonth;

        //    var salaries = await _context.Salaries
        //        .Where(s => s.Year == selectedYear && s.Month == selectedMonth)
        //        .ToListAsync();

        //    ViewData["Title"] = "Salary Page";
        //    ViewData["Years"] = new SelectList(years, selectedYear);
        //    ViewData["Months"] = new SelectList(months.Select((month, index) => new { Index = index + 1, Month = month }), "Index", "Month", selectedMonth);
        //    ViewData["SelectedYear"] = selectedYear;
        //    ViewData["SelectedMonth"] = selectedMonth;



        //    return View(salaries);
        //}

        //public async Task<IActionResult> Index(int? selectedYear, int? selectedMonth)
        //{

        //    int currentYear = DateTime.Now.Year;
        //    int currentMonth = DateTime.Now.Month;
        //    var employees = await _context.Employees.ToListAsync();
        //    // Получаем список всех уникальных годов из базы данных
        //    List<int> years = await _context.Salaries.Select(s => s.Year).Distinct().ToListAsync();

        //    // Создаем список названий месяцев на основе текущей культуры
        //    List<string> months = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();

        //    // Если пользователь не выбрал год и месяц, устанавливаем текущий год и месяц
        //    selectedYear ??= currentYear;
        //    selectedMonth ??= currentMonth;

        //    List<Salary> salaries = new List<Salary>(); // Определение списка зарплат
        //    // Получаем информацию о бюджете
        //    var budget = _context.Budgets.FirstOrDefault();
        //    // Проверяем, есть ли уже данные для выбранного месяца и года
        //    var existingSalaries = await _context.Salaries
        //        .Where(s => s.Year == selectedYear && s.Month == selectedMonth)
        //        .ToListAsync();

        //    if (existingSalaries.Any())
        //    {
        //        // Если данные уже существуют, обновляем их
        //        foreach (var salary in existingSalaries)
        //        {
        //            if (salary.Issued != 1)
        //            {
        //                // Обновляем значения зарплаты
        //                var employee = await _context.Employees.FindAsync(salary.EmployeeID);
        //                salary.SalaryAmount = employee.Salary;

        //                // Вычисляем ProductCount для сотрудника на выбранный год и месяц
        //                salary.ProductCount = await _context.ProductManufacturings
        //                    .Where(pm => pm.EmployeeID == salary.EmployeeID &&
        //                                 pm.Date.Year == selectedYear &&
        //                                 pm.Date.Month == selectedMonth)
        //                    .CountAsync();

        //                // Вычисляем SalesCount для сотрудника на выбранный год и месяц
        //                salary.SalesCount = await _context.SaleProducts
        //                    .Where(sp => sp.EmployeeID == salary.EmployeeID &&
        //                                 sp.Date.Year == selectedYear &&
        //                                 sp.Date.Month == selectedMonth)
        //                    .CountAsync();

        //                // Вычисляем PurchaseCount для сотрудника на выбранный год и месяц
        //                salary.PurchaseCount = await _context.PurchaseRawMaterials
        //                    .Where(prm => prm.EmployeeID == salary.EmployeeID &&
        //                                  prm.Date.Year == selectedYear &&
        //                                  prm.Date.Month == selectedMonth)
        //                    .CountAsync();

        //                // Вычисляем общее количество (CommonCount)
        //                salary.CommonCount = salary.ProductCount + salary.SalesCount + salary.PurchaseCount;

        //                // Вычисляем значение General
        //                double bonus2 = salary.CommonCount * budget.Bonus / 100;

        //                // Устанавливаем значение бонуса (Bonus) из бюджета
        //                salary.Bonus = bonus2 * employee.Salary;

        //                // Преобразуем значение бонуса (Bonus) в строку с форматированием
        //                salary.Bonus = Math.Round(salary.Bonus, 2); // Округляем до двух знаков после запятой
        //                string bonusString = salary.Bonus.ToString("0.00", CultureInfo.InvariantCulture);
        //                salary.Bonus = Convert.ToDouble(bonusString, CultureInfo.InvariantCulture);

        //                // Вычисляем значение General
        //                salary.General = (bonus2 + 1) * employee.Salary;

        //                // Преобразуем значение General в строку с форматированием
        //                salary.General = Math.Round(salary.General, 2); // Округляем до двух знаков после запятой
        //                string generalString = salary.General.ToString("0.00", CultureInfo.InvariantCulture);
        //                salary.General = Convert.ToDouble(generalString, CultureInfo.InvariantCulture);

        //                //// Устанавливаем значение Issued
        //                //salary.Issued = 0;
        //                // Обновляем данные в контексте
        //                _context.Salaries.Update(salary);
        //            }


        //        }
        //    }
        //    else
        //    {
        //        foreach (var employee in employees)
        //        {
        //            Salary salary = new Salary
        //            {
        //                Year = selectedYear.Value,
        //                Month = selectedMonth.Value,
        //                EmployeeID = employee.EmployeeId,
        //                SalaryAmount = employee.Salary
        //                // Добавьте вычисления остальных значений зарплаты
        //            };

        //            salary.SalaryAmount = employee.Salary;

        //            // Вычисляем ProductCount для сотрудника на выбранный год и месяц
        //            salary.ProductCount = await _context.ProductManufacturings
        //                .Where(pm => pm.EmployeeID == salary.EmployeeID &&
        //                             pm.Date.Year == selectedYear &&
        //                             pm.Date.Month == selectedMonth)
        //                .CountAsync();

        //            // Вычисляем SalesCount для сотрудника на выбранный год и месяц
        //            salary.SalesCount = await _context.SaleProducts
        //                .Where(sp => sp.EmployeeID == salary.EmployeeID &&
        //                             sp.Date.Year == selectedYear &&
        //                             sp.Date.Month == selectedMonth)
        //                .CountAsync();

        //            // Вычисляем PurchaseCount для сотрудника на выбранный год и месяц
        //            salary.PurchaseCount = await _context.PurchaseRawMaterials
        //                .Where(prm => prm.EmployeeID == salary.EmployeeID &&
        //                              prm.Date.Year == selectedYear &&
        //                              prm.Date.Month == selectedMonth)
        //                .CountAsync();

        //            // Вычисляем общее количество (CommonCount)
        //            salary.CommonCount = salary.ProductCount + salary.SalesCount + salary.PurchaseCount;

        //            // Вычисляем значение General
        //            double bonus2 = salary.CommonCount * budget.Bonus / 100;

        //            // Устанавливаем значение бонуса (Bonus) из бюджета
        //            salary.Bonus = bonus2 * employee.Salary;

        //            // Преобразуем значение бонуса (Bonus) в строку с форматированием
        //            salary.Bonus = Math.Round(salary.Bonus, 2); // Округляем до двух знаков после запятой
        //            string bonusString = salary.Bonus.ToString("0.00", CultureInfo.InvariantCulture);
        //            salary.Bonus = Convert.ToDouble(bonusString, CultureInfo.InvariantCulture);

        //            // Вычисляем значение General
        //            salary.General = (bonus2 + 1) * employee.Salary;

        //            // Преобразуем значение General в строку с форматированием
        //            salary.General = Math.Round(salary.General, 2); // Округляем до двух знаков после запятой
        //            string generalString = salary.General.ToString("0.00", CultureInfo.InvariantCulture);
        //            salary.General = Convert.ToDouble(generalString, CultureInfo.InvariantCulture);

        //            // Устанавливаем значение Issued
        //            salary.Issued = 0;
        //            // Добавляем созданную зарплату в список
        //            salaries.Add(salary);
        //        }

        //        // Добавляем новые данные зарплаты в базу данных
        //        _context.Salaries.AddRange(salaries);
        //    }

        //    await _context.SaveChangesAsync();



        //    // Передаем данные в представление для отображения
        //    ViewData["Title"] = "Salary Page";
        //    ViewData["Years"] = new SelectList(years, selectedYear);
        //    ViewData["Months"] = new SelectList(months.Select((month, index) => new { Index = index + 1, Month = month }), "Index", "Month", selectedMonth);
        //    ViewData["SelectedYear"] = selectedYear;
        //    ViewData["SelectedMonth"] = selectedMonth;
        //    // Filtering the salaries with Issued equal to 1 and returning them if found

        //    // Возвращаем список зарплат для отображения в представлении
        //    return View(existingSalaries.Any() ? existingSalaries : salaries);
        //}

        public async Task<IActionResult> Index(int? selectedYear, int? selectedMonth)
        {
            List<int> years = Enumerable.Range(2022, 13).ToList();
            //List<int> years = await _context.Salaries.Select(s => s.Year).Distinct().ToListAsync();
            List<string> months = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();
            if (selectedYear == null || selectedMonth == null)
            {
                // Пользователь не выбрал месяц и/или год, не выполняем никаких операций
                ViewData["Title"] = "Salary Page";
                ViewData["Years"] = new SelectList(years, selectedYear);
                // ViewData["Years"] = new SelectList(years, selectedYear);
                ViewData["Months"] = new SelectList(months.Select((month, index) => new { Index = index + 1, Month = month }), "Index", "Month", selectedMonth);
                return View(new List<Salary>());
            }
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            var employees = await _context.Employees.ToListAsync();
           

            selectedYear ??= currentYear;
            selectedMonth ??= currentMonth;

            var existingSalaries = await _context.Salaries
                .Where(s => s.Year == selectedYear && s.Month == selectedMonth)
                .ToListAsync();

            if (existingSalaries.Any())
            {
                foreach (var salary in existingSalaries)
                {
                   if  (salary.Issued != 1)
                    {
                        var employee = await _context.Employees.FindAsync(salary.EmployeeID);
                        salary.SalaryAmount = employee.Salary;

                        salary.ProductCount = await _context.ProductManufacturings
                            .Where(pm => pm.EmployeeID == salary.EmployeeID &&
                                         pm.Date.Year == selectedYear &&
                                         pm.Date.Month == selectedMonth)
                            .CountAsync();

                        salary.SalesCount = await _context.SaleProducts
                            .Where(sp => sp.EmployeeID == salary.EmployeeID &&
                                         sp.Date.Year == selectedYear &&
                                         sp.Date.Month == selectedMonth)
                            .CountAsync();

                        salary.PurchaseCount = await _context.PurchaseRawMaterials
                            .Where(prm => prm.EmployeeID == salary.EmployeeID &&
                                          prm.Date.Year == selectedYear &&
                                          prm.Date.Month == selectedMonth)
                            .CountAsync();

                        salary.CommonCount = salary.ProductCount + salary.SalesCount + salary.PurchaseCount;

                        var budget = _context.Budgets.FirstOrDefault();
                        double bonus2 = salary.CommonCount * budget.Bonus / 100;

                        salary.Bonus = bonus2 * employee.Salary;
                        salary.Bonus = Math.Round(salary.Bonus, 2);
                        string bonusString = salary.Bonus.ToString("0.00", CultureInfo.InvariantCulture);
                        salary.Bonus = Convert.ToDouble(bonusString, CultureInfo.InvariantCulture);

                        //salary.General = (bonus2 + 1) * employee.Salary;
                        salary.General = Math.Round(salary.General, 2);
                        string generalString = salary.General.ToString("0.00", CultureInfo.InvariantCulture);
                        salary.General = Convert.ToDouble(generalString, CultureInfo.InvariantCulture);

                        _context.Salaries.Update(salary);
                    }
                }
            }
            else
            {
                foreach (var employee in employees)
                {
                    Salary salary = new Salary
                    {
                        Year = selectedYear.Value,
                        Month = selectedMonth.Value,
                        EmployeeID = employee.EmployeeId,
                        SalaryAmount = employee.Salary
                    };

                    salary.ProductCount = await _context.ProductManufacturings
                        .Where(pm => pm.EmployeeID == salary.EmployeeID &&
                                     pm.Date.Year == selectedYear &&
                                     pm.Date.Month == selectedMonth)
                        .CountAsync();

                    salary.SalesCount = await _context.SaleProducts
                        .Where(sp => sp.EmployeeID == salary.EmployeeID &&
                                     sp.Date.Year == selectedYear &&
                                     sp.Date.Month == selectedMonth)
                        .CountAsync();

                    salary.PurchaseCount = await _context.PurchaseRawMaterials
                        .Where(prm => prm.EmployeeID == salary.EmployeeID &&
                                      prm.Date.Year == selectedYear &&
                                      prm.Date.Month == selectedMonth)
                        .CountAsync();

                    salary.CommonCount = salary.ProductCount + salary.SalesCount + salary.PurchaseCount;

                    var budget = _context.Budgets.FirstOrDefault();
                    double bonus2 = salary.CommonCount * budget.Bonus / 100;

                    salary.Bonus = bonus2 * employee.Salary;
                    salary.Bonus = Math.Round(salary.Bonus, 2);
                    string bonusString = salary.Bonus.ToString("0.00", CultureInfo.InvariantCulture);
                    salary.Bonus = Convert.ToDouble(bonusString, CultureInfo.InvariantCulture);

                    salary.General = (bonus2 + 1) * employee.Salary;
                    salary.General = Math.Round(salary.General, 2);
                    string generalString = salary.General.ToString("0.00", CultureInfo.InvariantCulture);
                    salary.General = Convert.ToDouble(generalString, CultureInfo.InvariantCulture);

                    salary.Issued = 0;
                    _context.Salaries.Add(salary);
                }
            }

            await _context.SaveChangesAsync();

            // Retrieve all salaries for the selected year and month
            var allSalaries = await _context.Salaries
                .Where(s => s.Year == selectedYear && s.Month == selectedMonth)
                .ToListAsync();

            ViewData["Title"] = "Salary Page";
            ViewData["Years"] = new SelectList(years, selectedYear);
           // ViewData["Years"] = new SelectList(years, selectedYear);
            ViewData["Months"] = new SelectList(months.Select((month, index) => new { Index = index + 1, Month = month }), "Index", "Month", selectedMonth);
            ViewData["SelectedYear"] = selectedYear;
            ViewData["SelectedMonth"] = selectedMonth;

            return View(allSalaries);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null)
            {
                return NotFound();
            }

            return View(salary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,General,Issued")] Salary salary)
        {
            if (id != salary.ID)
            {
                return NotFound();
            }

            // Получаем существующую запись о зарплате
            var existingSalary = await _context.Salaries.FindAsync(id);

            if (existingSalary == null)
            {
                return NotFound();
            }

            // Получаем информацию о бюджете
            var budget = await _context.Budgets.FirstOrDefaultAsync();

            if (budget == null)
            {
                return NotFound(); // Можете сделать обработку отсутствия бюджета в вашем приложении
            }

            //// Проверяем, хватает ли бюджета для обновления зарплаты
            //if (salary.General > budget.BudgetAmount)
            //{
            //    // Если бюджета недостаточно, возвращаем сообщение
            //    ModelState.AddModelError(string.Empty, "Not enough budget for this salary.");
            //    return View(salary);
            //}

            //// Если Issued = 1, вычитаем General из BudgetAmount
            //if (salary.Issued == 1)
            //{
            //    budget.BudgetAmount -= salary.General;
            //}

            // Обновляем только General и Issued
            existingSalary.General = salary.General;
            existingSalary.Issued = salary.Issued;

            //// Вычитаем сумму General из бюджета
            //budget.BudgetAmount -= salary.General;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            // Перенаправляем пользователя на страницу Index с параметрами года и месяца редактируемой зарплаты
            return RedirectToAction(nameof(Index), new { selectedYear = existingSalary.Year, selectedMonth = existingSalary.Month });
        }


        private bool SalaryExists(int id)
        {
            return _context.Salaries.Any(e => e.ID == id);
        }

        public async Task<IActionResult> IssuedAll(int selectedYear, int selectedMonth)
        {
            var salariesToBeIssued = await _context.Salaries
                .Where(s => s.Year == selectedYear && s.Month == selectedMonth && s.Issued == 0)
                .ToListAsync();

            // Calculate total General for the selected month and year
            double totalGeneral = salariesToBeIssued.Sum(s => s.General);

            // Get the budget
            var budget = await _context.Budgets.FirstOrDefaultAsync();

            // Check if the budget is sufficient
            if (totalGeneral > budget.BudgetAmount)
            {
                // If the budget is not enough, return an error message
                TempData["ErrorMessage"] = "Not enough budget to issue all salaries.";
                return RedirectToAction(nameof(Index), new { selectedYear, selectedMonth });
            }

            // Subtract General from BudgetAmount for each salary and set Issued to 1
            foreach (var salary in salariesToBeIssued)
            {
                budget.BudgetAmount -= salary.General;
                salary.Issued = 1;
                _context.Salaries.Update(salary);
            }

            await _context.SaveChangesAsync();

            // Redirect back to the Index action with the selected year and month
            return RedirectToAction(nameof(Index), new { selectedYear, selectedMonth });
        }

    }
}
