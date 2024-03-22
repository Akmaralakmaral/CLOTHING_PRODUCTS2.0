using CLOTHING_PRODUCTS.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Create()
        {
            // Получаем список завершенных продуктов из хранимой процедуры
            var finishedProducts = _dbContext.FinishedProducts.FromSqlRaw("EXEC GetFinishedProducts").ToList();

            // Получаем список сотрудников из хранимой процедуры
            var employees = _dbContext.Employees.FromSqlRaw("EXEC GetEmployees").ToList();

            ViewBag.FinishedProducts = finishedProducts;
            ViewBag.Employees = employees;

            // В этом методе вы можете передать необходимые данные в представление
            // Например, список завершенных продуктов, сотрудников и т.д.
            return View();
        }


    }
}
