
using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class SaleProductController : Controller
    {
        private readonly AddDBContext _dbContext;

        public SaleProductController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var saleProducts = await _dbContext.SaleProducts
                .Include(e => e.FinishedProduct)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(saleProducts);
        }
        public IActionResult Create()
        {
            ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
            ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName");
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("FinishedProductID, Quantity, Amount, Date, EmployeeID")] SaleProduct saleProduct)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _dbContext.Add(saleProduct);
        //        await _dbContext.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name", saleProduct.FinishedProductID);
        //    ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", saleProduct.EmployeeID);
        //    return View(saleProduct);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FinishedProductID, Quantity, Date, EmployeeID")] SaleProduct saleProduct)
        {
          
            
            var finishedProduct = await _dbContext.FinishedProducts.FindAsync(saleProduct.FinishedProductID);

            
             if (finishedProduct.Quantity < saleProduct.Quantity)
            {
                ViewData["InsufficientQuantityMessage"] = "Недостаточно товара на складе.";
                ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
                ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", saleProduct.EmployeeID);
                return View(saleProduct);
            }
 
            
           

            var budget = await _dbContext.Budgets.FirstOrDefaultAsync();

            var sebestiomost = (saleProduct.Quantity * (finishedProduct.Amount / finishedProduct.Quantity));
             var saleAmount = sebestiomost *( ((budget.Percent) / 100)+1);
 
            
            budget.BudgetAmount += saleAmount;
            saleProduct.Amount = saleAmount;
            finishedProduct.Amount -= sebestiomost;
            finishedProduct.Quantity -= saleProduct.Quantity;
            _dbContext.Add(saleProduct);

            
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
          
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
                var saleReportData = await _dbContext.SaleProducts
                    .FromSqlInterpolated($"EXEC [dbo].[SaleReport] {startDate}, {endDate}")
                    .ToListAsync();

                // Загружаем связанные данные о сотрудниках и продуктах для каждого объекта SaleProduct
                foreach (var saleProduct in saleReportData)
                {
                    _dbContext.Entry(saleProduct)
                        .Reference(sp => sp.Employee)
                        .Load();

                    _dbContext.Entry(saleProduct)
                        .Reference(sp => sp.FinishedProduct)
                        .Load();
                }   
           
            
            return View(saleReportData);
        }



        public IActionResult DownloadPdf(DateTime startDate, DateTime endDate)
        {
            // Создаем документ PDF
            var document = new PdfDocument();
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            // Отображаем заголовок и интервал времени в начале документа
            graphics.DrawString("Sale Report", font, XBrushes.Black, new XRect(50, 30, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);
            graphics.DrawString($"Interval: {startDate.ToShortDateString()} - {endDate.ToShortDateString()}", font, XBrushes.Black, new XRect(50, 40, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);

            // Определяем ширину и высоту столбцов и строк таблицы
            double columnWidth = 100;
            double rowHeight = 20;

            // Отображаем заголовки таблицы в PDF
            int yPos = 70; // Начальная позиция вертикального расположения

            // Заголовки столбцов
            string[] columnHeaders = { "Employee Name", "Product Name", "Quantity", "Amount", "Date" };

            // Отображаем заголовки таблицы
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                graphics.DrawRectangle(XBrushes.LightGray, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight));
                graphics.DrawString(columnHeaders[i], font, XBrushes.Black, new XRect(50 + i * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
            }

            // Увеличиваем позицию для следующей строки
            yPos += (int)rowHeight;

            // Выполняем хранимую процедуру и получаем данные
            var saleReportData = _dbContext.SaleProducts
                .FromSqlInterpolated($"EXEC [dbo].[SaleReport] {startDate}, {endDate}")
                .ToList();

            // Заполняем таблицу данными
            foreach (var item in saleReportData)
            {
                // Получаем имя сотрудника и название продукта
                var employeeName = GetEmployeeNameById(item.EmployeeID);
                var finishedProductName = GetFinishedProductNameById(item.FinishedProductID);

                // Отображаем данные в соответствующих столбцах таблицы
                string[] rowData = { employeeName, finishedProductName, item.Quantity.ToString(), item.Amount.ToString("N2"), item.Date.ToShortDateString() };

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
            graphics.DrawString(saleReportData.Sum(item => item.Quantity).ToString(), font, XBrushes.Black, new XRect(50 + 2 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);
            graphics.DrawString(saleReportData.Sum(item => item.Amount).ToString("N2"), font, XBrushes.Black, new XRect(50 + 3 * columnWidth, yPos, columnWidth, rowHeight), XStringFormats.Center);

            // Сохраняем документ в поток
            var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Position = 0;

            // Возвращаем PDF как файл для скачивания
            return File(memoryStream, "application/pdf", "sale_report.pdf");
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
