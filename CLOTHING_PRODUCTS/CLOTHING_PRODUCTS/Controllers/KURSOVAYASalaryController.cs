using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Data;
using System.Globalization;

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
                    var issuedStatus = _context.Salaries
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => s.Issued)
                        .FirstOrDefault();

                    if (issuedStatus == 0)
                    {
                        _context.Database.ExecuteSqlRaw("EXEC [dbo].[CreateSalary] @Year, @Month",
                           new Microsoft.Data.SqlClient.SqlParameter("@Year", year),
                           new Microsoft.Data.SqlClient.SqlParameter("@Month", month));
                    }
                    
                    var salaries = _context.Salaries.FromSqlRaw("EXEC [dbo].[GetSalaries] @Year, @Month",
                        new Microsoft.Data.SqlClient.SqlParameter("@Year", year),
                        new Microsoft.Data.SqlClient.SqlParameter("@Month", month)).ToList();
                    var totalGeneral = _context.Salaries
                    .Where(s => s.Year == year && s.Month == month)
                    .Sum(s => s.General);
                    ViewBag.SelectedYear = year;
                    ViewBag.SelectedMonth = month;
                    ViewBag.TotalGeneral = totalGeneral;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateIssuedStatus(int year, int month)
        {
            // Переменная для хранения результата хранимой процедуры
            int result = 0;

            // Вызов хранимой процедуры UpdateIssuedStatus
            SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            _context.Database.ExecuteSqlRaw("EXEC [dbo].[UpdateIssuedStatus] @Year, @Month, @Result OUTPUT",
                new SqlParameter("@Year", year),
                new SqlParameter("@Month", month),
                resultParam);

            // Получаем значение переменной result после выполнения процедуры
            result = (int)resultParam.Value;
            // Проверка результата и выполнение соответствующих действий
            bool fromEditGeneral = (result == 1);
            if (result == 2)
            {
                // If the budget is not enough, return an error message
                TempData["ErrorMessage"] = "Not enough budget to issue all salaries.";
            }

            // Возвращаем пользователя на страницу Index
            return RedirectToAction(nameof(Index), new { year = year, month = month, fromEditGeneral = fromEditGeneral });
        }

        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null)
            {
                startDate = DateTime.Today; // Устанавливаем сегодняшнюю дату по умолчанию
            }

            if (endDate == null)
            {
                endDate = DateTime.Today; // Устанавливаем сегодняшнюю дату по умолчанию
            }

            ViewBag.SelectedStartDate = startDate;
            ViewBag.SelectedEndDate = endDate;

            // Выполняем запрос к хранимой процедуре без использования Include
            var salariesReportData = await _context.Salaries
                .FromSqlInterpolated($"EXEC [dbo].[SalariesReport] {startDate}, {endDate}")
                .ToListAsync();

            // Загружаем связанные данные о сотрудниках и продуктах для каждого объекта SaleProduct
            foreach (var salariesEmployee in salariesReportData)
            {
                _context.Entry(salariesEmployee)
                    .Reference(sp => sp.Employee)
                    .Load();

            }


            return View(salariesReportData);
        }



      public IActionResult DownloadPdf(DateTime startDate, DateTime endDate)
{
    // Создаем документ PDF с форматом страницы A4
    var document = new PdfDocument();
    var page = document.AddPage();
    page.Size = PageSize.A4; // Установка формата страницы A4
    var graphics = XGraphics.FromPdfPage(page);
    var font = new XFont("Arial", 10, XFontStyle.Regular); // Уменьшаем размер шрифта

    // Отображаем заголовок и интервал времени в начале документа
    graphics.DrawString("Salary Report", font, XBrushes.Black, new XRect(30, 30, page.Width.Point, 20), XStringFormats.TopCenter);
    graphics.DrawString($"Interval: {startDate.ToShortDateString()} - {endDate.ToShortDateString()}", font, XBrushes.Black, new XRect(30, 45, page.Width.Point, 20), XStringFormats.TopCenter);

    // Определяем ширину и высоту столбцов и строк таблицы
    double columnWidth = 50; // Уменьшаем ширину столбцов
    double rowHeight = 15; // Уменьшаем высоту строк

    // Отображаем заголовки таблицы в PDF
    int yPos = 70; // Начальная позиция вертикального расположения

    // Заголовки столбцов
    string[] columnHeaders = { "Year", "Month", "Employee", "Purchase", "Product", "Sales", "Common", "Salary Amount", "Bonus", "General", "Issued" };

    // Отображаем заголовки таблицы
    for (int i = 0; i < columnHeaders.Length; i++)
    {
        graphics.DrawRectangle(XBrushes.LightGray, new XRect(30 + i * columnWidth, yPos, columnWidth, rowHeight));
        graphics.DrawString(columnHeaders[i], font, XBrushes.Black, new XRect(30 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    }

    // Увеличиваем позицию для следующей строки
    yPos += (int)rowHeight;

    // Выполняем хранимую процедуру и получаем данные
    var manufacturingReportData = _context.Salaries
        .FromSqlInterpolated($"EXEC [dbo].[SalariesReport] {startDate}, {endDate}")
        .ToList();

    // Заполняем таблицу данными
    foreach (var item in manufacturingReportData)
    {
        // Получаем имя сотрудника и название продукта
        var employeeName = GetEmployeeNameById(item.EmployeeID);

        // Отображаем данные в соответствующих столбцах таблицы
        string[] rowData = {item.Year.ToString(), @CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month), employeeName, item.PurchaseCount.ToString(), item.ProductCount.ToString(), item.SalesCount.ToString(),
        item.CommonCount.ToString(), item.SalaryAmount.ToString(), item.Bonus.ToString(), item.General.ToString(),item.Issued.ToString() };

        for (int i = 0; i < rowData.Length; i++)
        {
            graphics.DrawRectangle(XBrushes.White, new XRect(30 + i * columnWidth, yPos, columnWidth, rowHeight));
            graphics.DrawString(rowData[i], font, XBrushes.Black, new XRect(30 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
        }

        // Увеличиваем позицию для следующей строки
        yPos += (int)rowHeight;
    }

    // Добавляем строку с итогами
    graphics.DrawRectangle(XBrushes.LightGray, new XRect(30, yPos, columnWidth, rowHeight));
    graphics.DrawString("Total:", font, XBrushes.Black, new XRect(30, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.PurchaseCount).ToString(), font, XBrushes.Black, new XRect(30 + 3 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.ProductCount).ToString(), font, XBrushes.Black, new XRect(30 + 4 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.SalesCount).ToString(), font, XBrushes.Black, new XRect(30 + 5 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.CommonCount).ToString(), font, XBrushes.Black, new XRect(30 + 6 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.SalaryAmount).ToString(), font, XBrushes.Black, new XRect(30 + 7 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.Bonus).ToString(), font, XBrushes.Black, new XRect(30 + 8 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.General).ToString(), font, XBrushes.Black, new XRect(30 + 9 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
    graphics.DrawString(manufacturingReportData.Sum(item => item.Issued).ToString(), font, XBrushes.Black, new XRect(30 + 10 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);

    // Сохраняем документ в поток
    var memoryStream = new MemoryStream();
    document.Save(memoryStream);
    memoryStream.Position = 0;

    // Возвращаем PDF как файл для скачивания
    return File(memoryStream, "application/pdf", "salary_report.pdf");
}




        private string GetEmployeeNameById(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            return employee != null ? employee.FullName : " ";
        }







    }
}
    
