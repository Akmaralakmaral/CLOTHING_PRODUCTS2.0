using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class ProductManufacturingController : Controller
    {
        private readonly AddDBContext _dbContext;

        public ProductManufacturingController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var productManufacturings = await _dbContext.ProductManufacturings
                .Include(e => e.FinishedProduct)
                .Include(e => e.Employee)
                .ToListAsync();

            return View(productManufacturings);
        }

        public IActionResult Create()
        {
            ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name");
            ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductManufacturing productManufacturing)
        {
           
            var finishedProduct = await _dbContext.FinishedProducts
                .Include(fp => fp.Ingredients)
                .FirstOrDefaultAsync(fp => fp.FinishedProductId == productManufacturing.FinishedProductID);

            // Проверка на наличие продукта
            if (finishedProduct == null)
            {
                ModelState.AddModelError(string.Empty, "Выбранный продукт не найден");
                return View(productManufacturing);
            }

            // Проверка на наличие необходимого сырья
            foreach (var ingredient in finishedProduct.Ingredients)
            {
                var rawMaterial = await _dbContext.RawMaterials.FindAsync(ingredient.RawMaterialId);
                 if (rawMaterial.Quantity < ingredient.Quantity * productManufacturing.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Недостаточно сырья: {rawMaterial.Name}");
                    ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name", productManufacturing.FinishedProductID);
                    ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", productManufacturing.EmployeeID);
                    return View(productManufacturing);
                }   
             }
            double productAmount = 0;
            // Вычитание необходимого сырья
            foreach (var ingredient in finishedProduct.Ingredients)
            {
                var rawMaterial = await _dbContext.RawMaterials.FindAsync(ingredient.RawMaterialId);
                double rawMaterialQuantity = ingredient.Quantity * productManufacturing.Quantity;
                 double rawMaterialAmount = (rawMaterial.Amount / rawMaterial.Quantity)*(rawMaterialQuantity);
                 
                rawMaterial.Amount -= rawMaterialAmount;
                rawMaterial.Quantity -= rawMaterialQuantity;
                productAmount = productAmount + rawMaterialAmount;

            }

            // Увеличение количества продукции на складе
            finishedProduct.Quantity += productManufacturing.Quantity;
            finishedProduct.Amount += productAmount; // Предполагая, что стоимость увеличивается на сумму всех сырья

            // Создание записи о производстве продукта
            _dbContext.ProductManufacturings.Add(productManufacturing);

            // Сохранение изменений в базе данных
            await _dbContext.SaveChangesAsync();

                
            

            ViewBag.FinishedProducts = new SelectList(_dbContext.FinishedProducts, "FinishedProductId", "Name", productManufacturing.FinishedProductID);
            ViewBag.Employees = new SelectList(_dbContext.Employees, "EmployeeId", "FullName", productManufacturing.EmployeeID);
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



    }
}
