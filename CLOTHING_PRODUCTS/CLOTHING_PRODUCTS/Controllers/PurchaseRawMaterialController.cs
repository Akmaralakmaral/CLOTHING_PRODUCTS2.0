using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class PurchaseRawMaterialController : Controller
    {
        private readonly AddDBContext _dbContext;

        public PurchaseRawMaterialController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var purchaseRawMaterials = await _dbContext.PurchaseRawMaterials
                .Include(e => e.RawMaterial)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(purchaseRawMaterials);
        }

        public IActionResult Create()
        {
            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
            ViewBag.Employees = _dbContext.Employees.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RawMaterialID,Quantity,Amount,Date,EmployeeID")] PurchaseRawMaterial purchaseRawMaterial)
        {           
            // Проверяем, хватит ли бюджета для закупки данного сырья на указанную сумму
            var budget = await _dbContext.Budgets.FirstOrDefaultAsync();

            if (budget != null && budget.BudgetAmount >= purchaseRawMaterial.Amount)
            {
                // Уменьшаем бюджет на сумму закупки
                budget.BudgetAmount -= purchaseRawMaterial.Amount;

                // Увеличиваем склад сырья на закупаемое количество
                var rawMaterial = await _dbContext.RawMaterials.FindAsync(purchaseRawMaterial.RawMaterialID);
                if (rawMaterial != null)
                {
                    rawMaterial.Quantity += purchaseRawMaterial.Quantity;
                    rawMaterial.Amount += purchaseRawMaterial.Amount;
                }

                // Добавляем запись о закупке сырья
                _dbContext.Add(purchaseRawMaterial);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                ViewBag.Employees = _dbContext.Employees.ToList();
                // Если бюджет не позволяет купить сырье на данную сумму, возвращаем ошибку
                ModelState.AddModelError(string.Empty, "Недостаточно средств в бюджете для закупки сырья");
                return View(new PurchaseRawMaterial()); // Возвращаем представление с пустой моделью


                //// Если бюджет не позволяет купить сырье на данную сумму, возвращаем ошибку
                //ModelState.AddModelError(string.Empty, "Недостаточно средств в бюджете для закупки сырья");
                //return View(purchaseRawMaterial);
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
            var purchaseReportData = await _dbContext.PurchaseRawMaterials
                .FromSqlInterpolated($"EXEC [dbo].[PurchaseReport] {startDate}, {endDate}")
                .ToListAsync();

            // Загружаем связанные данные о сотрудниках и продуктах для каждого объекта SaleProduct
            foreach (var purchaseRawMaterial in purchaseReportData)
            {
                _dbContext.Entry(purchaseRawMaterial)
                    .Reference(sp => sp.Employee)
                    .Load();

                _dbContext.Entry(purchaseRawMaterial)
                    .Reference(sp => sp.RawMaterial)
                    .Load();
            }


            return View(purchaseReportData);
        }

        public IActionResult DownloadPdf(DateTime startDate, DateTime endDate)
        {
            // Создаем документ PDF
            var document = new PdfDocument();
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            // Отображаем заголовок и интервал времени в начале документа
            graphics.DrawString("Purchase Report", font, XBrushes.Black, new XRect(50, 30, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);
            graphics.DrawString($"Interval: {startDate.ToShortDateString()} - {endDate.ToShortDateString()}", font, XBrushes.Black, new XRect(50, 40, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);

            // Определяем ширину и высоту столбцов и строк таблицы
            double columnWidth = 100;
            double rowHeight = 20;

            // Отображаем заголовки таблицы в PDF
            int yPos = 70; // Начальная позиция вертикального расположения

            // Заголовки столбцов
            string[] columnHeaders = { "Employee Name", "Purchase Name", "Quantity", "Amount", "Date" };

            // Отображаем заголовки таблицы
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                graphics.DrawRectangle(XBrushes.LightGray, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight));
                graphics.DrawString(columnHeaders[i], font, XBrushes.Black, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
            }

            // Увеличиваем позицию для следующей строки
            yPos += (int)rowHeight;

            // Выполняем хранимую процедуру и получаем данные
            var purchaseReportData = _dbContext.PurchaseRawMaterials
                .FromSqlInterpolated($"EXEC [dbo].[PurchaseReport] {startDate}, {endDate}")
                .ToList();

            // Заполняем таблицу данными
            foreach (var item in purchaseReportData)
            {
                // Получаем имя сотрудника и название продукта
                var employeeName = GetEmployeeNameById(item.EmployeeID);
                var rawMaterialName = GetRawMaterialNameById(item.RawMaterialID);

                // Отображаем данные в соответствующих столбцах таблицы
                string[] rowData = { employeeName, rawMaterialName, item.Quantity.ToString(), item.Amount.ToString("N2"), item.Date.ToShortDateString() };

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
            graphics.DrawString(purchaseReportData.Sum(item => item.Quantity).ToString(), font, XBrushes.Black, new XRect(50 + 2 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
            graphics.DrawString(purchaseReportData.Sum(item => item.Amount).ToString("N2"), font, XBrushes.Black, new XRect(50 + 3 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);

            // Сохраняем документ в поток
            var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;

            // Возвращаем PDF как файл для скачивания
            return File(memoryStream, "application/pdf", "purchase_report.pdf");
        }





        private string GetRawMaterialNameById(int rawMaterialID)
        {
            var rawMaterial = _dbContext.RawMaterials.FirstOrDefault(e => e.RawMaterialId == rawMaterialID);
            return rawMaterial != null ? rawMaterial.Name : " ";
        }

        private string GetEmployeeNameById(int employeeId)
        {
            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            return employee != null ? employee.FullName : " ";
        }


    }
}
