using CLOTHING_PRODUCTS.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Data;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYAProductManufacturingController : Controller
    {

        private readonly AddDBContext _dbContext;

        public KURSOVAYAProductManufacturingController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            // Вызываем хранимую процедуру и получаем результат
            var productManufacturings = _dbContext.ProductManufacturings
                .FromSqlRaw("EXEC GetAllProductManufacturings")
                .ToList();

            // Передаем результат в представление
            return View(productManufacturings);
        }

        // GET: Отобразить форму для создания нового ProductManufacturing
        public IActionResult Create(int? finishedProductId = null, float? quantity = null, DateTime? date = null, int? employeeId = null)
        {
            // Получаем список завершенных продуктов из хранимой процедуры
            var finishedProducts = _dbContext.FinishedProducts.FromSqlRaw("EXEC GetFinishedProducts").ToList();

            // Получаем список сотрудников из хранимой процедуры
            var employees = _dbContext.Employees.FromSqlRaw("EXEC GetEmployees").ToList();

            ViewBag.FinishedProducts = finishedProducts;
            ViewBag.Employees = employees;

            // Передаем введенные значения в представление
            ViewBag.FinishedProductId = finishedProductId;
            ViewBag.Quantity = quantity;
            ViewBag.Date = date ?? DateTime.Today;
            ViewBag.EmployeeId = employeeId;

            return View();
        }

        // POST: Создать новый ProductManufacturing
        [HttpPost]
        public IActionResult Create(int finishedProductId, float quantity, DateTime date, int employeeId)
        {
            var availableRawMaterialParam = new SqlParameter("@available_raw_material", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            // Выполняем хранимую процедуру для проверки наличия достаточного количества сырья
            _dbContext.Database.ExecuteSqlRaw("EXEC SP_Production @id_product, @quantity, @date, @employee, @available_raw_material OUTPUT",
                new SqlParameter("@id_product", finishedProductId),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@date", date),
                new SqlParameter("@employee", employeeId),
                availableRawMaterialParam);

            int availableRawMaterial = (int)availableRawMaterialParam.Value;

            // Проверяем результат выполнения хранимой процедуры
            if (availableRawMaterial == 0)
            {
                // Если сырья недостаточно, возвращаем пользователю сообщение об ошибке
                TempData["ErrorMessage"] = "Not enough raw material available for production.";

                // Перенаправляем пользователя на страницу создания с передачей введенных значений
                return RedirectToAction("Create", new { finishedProductId = finishedProductId, quantity = quantity, date = date.ToString("yyyy-MM-dd"), employeeId = employeeId });

            }
            else
            {
                
                // Перенаправляем пользователя на страницу успешного создания записи
                return RedirectToAction("Index");
            }
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
            var manufacturingReportData = await _dbContext.ProductManufacturings
                .FromSqlInterpolated($"EXEC [dbo].[ManufacturingReport] {startDate}, {endDate}")
                .ToListAsync();

            // Загружаем связанные данные о сотрудниках и продуктах для каждого объекта SaleProduct
            foreach (var manufacturingProduct in manufacturingReportData)
            {
                _dbContext.Entry(manufacturingProduct)
                    .Reference(sp => sp.Employee)
                    .Load();

                _dbContext.Entry(manufacturingProduct)
                    .Reference(sp => sp.FinishedProduct)
                    .Load();
            }


            return View(manufacturingReportData);
        }



        public IActionResult DownloadPdf(DateTime startDate, DateTime endDate)
        {
            // Создаем документ PDF
            var document = new PdfDocument();
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            // Отображаем заголовок и интервал времени в начале документа
            graphics.DrawString("Manufacturing Report", font, XBrushes.Black, new XRect(50, 30, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);
            graphics.DrawString($"Interval: {startDate.ToShortDateString()} - {endDate.ToShortDateString()}", font, XBrushes.Black, new XRect(50, 40, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);

            // Определяем ширину и высоту столбцов и строк таблицы
            double columnWidth = 100;
            double rowHeight = 20;

            // Отображаем заголовки таблицы в PDF
            int yPos = 70; // Начальная позиция вертикального расположения

            // Заголовки столбцов
            string[] columnHeaders = { "Employee Name", "Product Name", "Quantity", "Date" };

            // Отображаем заголовки таблицы
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                graphics.DrawRectangle(XBrushes.LightGray, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight));
                graphics.DrawString(columnHeaders[i], font, XBrushes.Black, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
            }

            // Увеличиваем позицию для следующей строки
            yPos += (int)rowHeight;

            // Выполняем хранимую процедуру и получаем данные
            var manufacturingReportData = _dbContext.ProductManufacturings
                .FromSqlInterpolated($"EXEC [dbo].[ManufacturingReport] {startDate}, {endDate}")
                .ToList();

            // Заполняем таблицу данными
            foreach (var item in manufacturingReportData)
            {
                // Получаем имя сотрудника и название продукта
                var employeeName = GetEmployeeNameById(item.EmployeeID);
                var finishedProductName = GetFinishedProductNameById(item.FinishedProductID);

                // Отображаем данные в соответствующих столбцах таблицы
                string[] rowData = { employeeName, finishedProductName, item.Quantity.ToString(), item.Date.ToShortDateString() };

                for (int i = 0; i < rowData.Length; i++)
                {
                    graphics.DrawRectangle(XBrushes.White, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight));
                    graphics.DrawString(rowData[i], font, XBrushes.Black, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
                }

                // Увеличиваем позицию для следующей строки
                yPos += (int)rowHeight;
            }

            // Добавляем строку с итогами
            graphics.DrawRectangle(XBrushes.LightGray, new XRect(50, yPos, columnWidth, rowHeight));
            graphics.DrawString("Total:", font, XBrushes.Black, new XRect(50, yPos, columnWidth, rowHeight), XStringFormats.Center);
            graphics.DrawString(manufacturingReportData.Sum(item => item.Quantity).ToString(), font, XBrushes.Black, new XRect(50 + 2 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
           
            // Сохраняем документ в поток
            var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;

            // Возвращаем PDF как файл для скачивания
            return File(memoryStream, "application/pdf", "manufacturing_report.pdf");
        }





        private string GetFinishedProductNameById(int finishedProductID)
        {
            var finishedProduct = _dbContext.FinishedProducts.FirstOrDefault(e => e.FinishedProductId == finishedProductID);
            return finishedProduct != null ? finishedProduct.Name : " ";
        }

        private string GetEmployeeNameById(int employeeId)
        {
            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            return employee != null ? employee.FullName : " ";
        }


    }
}
