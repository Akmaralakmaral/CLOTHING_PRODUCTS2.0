using CLOTHING_PRODUCTS.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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


    }
}
