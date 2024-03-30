using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class KURSOVAYAIngredientController : Controller
    {
        private readonly AddDBContext _dbContext;

        public KURSOVAYAIngredientController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Показать форму выбора продукта и список ингредиентов
        public async Task<IActionResult> Index(int? productId)
        {
            int? selectedProductId = productId;
            ViewData["SelectedProductId"] = selectedProductId;
            // Вызываем хранимую процедуру для получения списка продуктов
            List<FinishedProduct> products = await _dbContext.FinishedProducts.FromSqlRaw("EXEC GetFinishedProducts").ToListAsync();

            // Создаем пустой список ингредиентов
            List<Ingredient> ingredients = new List<Ingredient>();

            if (productId.HasValue)
            {
                // Если передан productId, вызываем хранимую процедуру для получения ингредиентов для выбранного продукта
                ingredients = await _dbContext.Ingredients
                    .FromSqlRaw("EXEC GetIngredientsByProductId @ProductId", new SqlParameter("@ProductId", productId))
                    .ToListAsync(); 
            }

            // Помещаем список ингредиентов в ViewData для передачи в представление
            ViewData["Ingredients"] = ingredients;

            // Помещаем список продуктов в ViewData для передачи в представление
            ViewData["Products"] = new SelectList(products, "FinishedProductId", "Name", productId);

            // Вызываем представление и передаем в него список продуктов для отображения формы выбора продукта и список ингредиентов для выбранного продукта (если productId задан)
            return View("Index", products);
        }


        // POST: Получить ингредиенты для выбранного продукта и отобразить их
        [HttpPost]
        public async Task<IActionResult> Index(int productId)
        {
            int? selectedProductId = productId;
            ViewData["SelectedProductId"] = selectedProductId;
            // Вызываем хранимую процедуру для получения ингредиентов для выбранного продукта
            List<Ingredient> ingredients = await _dbContext.Ingredients
                .FromSqlRaw("EXEC GetIngredientsByProductId @ProductId", new SqlParameter("@ProductId", productId))
                .ToListAsync();

            // Помещаем список ингредиентов в ViewData для передачи в представление
            ViewData["Ingredients"] = ingredients;

            // Вызываем представление и передаем в него список продуктов для отображения формы выбора продукта и список ингредиентов для выбранного продукта
            List<FinishedProduct> products = await _dbContext.FinishedProducts.FromSqlRaw("EXEC GetFinishedProducts").ToListAsync();
            ViewData["Products"] = new SelectList(products, "FinishedProductId", "Name", productId); // Устанавливаем выбранный продукт в выпадающем списке

            return View("Index", products);
        }



        // GET: Отобразить форму для создания ингредиента
        public IActionResult CreateIngredientForm(int productId, int? rawMaterialId = null, float? quantity = null)
        {
            // Получаем название выбранного продукта по его ID
            var productName = _dbContext.FinishedProducts
                                    .Where(p => p.FinishedProductId == productId)
                                    .Select(p => p.Name)
                                    .FirstOrDefault();

            // Получаем список сырья из хранимой процедуры
            var rawMaterials = _dbContext.RawMaterials.FromSqlRaw("EXEC GetRawMaterials").ToList();

            ViewBag.ProductName = productName;
            ViewBag.RawMaterials = rawMaterials;
            ViewBag.ProductId = productId;
            ViewBag.RawMaterialId = rawMaterialId; 
            ViewBag.Quantity = quantity;

            return View(productId);
        }

        // POST: Создать ингредиент
        [HttpPost]
        public IActionResult CreateIngredientForm(int productId, int rawMaterialId, float quantity)
        {
            var rowsAffected = _dbContext.Database.ExecuteSqlInterpolated($"EXEC CreateIngredient {productId}, {rawMaterialId}, {quantity}");

            if (rowsAffected == -1)
            {
                TempData["ErrorMessage"] = "Raw material already exists for this product.";
                // Перенаправляем пользователя на GET метод для отображения представления с сообщением об ошибке и передаем введенные значения
                return RedirectToAction("CreateIngredientForm", new { productId = productId, rawMaterialId = rawMaterialId, quantity = quantity });
            }

            // Иначе, если ингредиент успешно создан, перенаправляем пользователя на Index
            return RedirectToAction("Index", "KURSOVAYAIngredient", new { productId = productId });
        }

        // GET: Отобразить форму для редактирования ингредиента
        public IActionResult EditIngredient(int ingredientId)
        {
            // Получаем информацию об ингредиенте по его ID
            var ingredient = _dbContext.Ingredients
                .FromSqlRaw($"EXEC GetIngredientById {ingredientId}")
                .AsEnumerable()
                .FirstOrDefault();

            // Получаем название материала по его ID
            var rawMaterial = _dbContext.RawMaterials
                .FirstOrDefault(r => r.RawMaterialId == ingredient.RawMaterialId);

            // Устанавливаем название материала в свойство RawMaterialName
            ingredient.RawMaterialName = rawMaterial.Name;
            // Получаем список сырья из хранимой процедуры
            var rawMaterials = _dbContext.RawMaterials.FromSqlRaw("EXEC GetRawMaterials").ToList();

            ViewBag.RawMaterials = rawMaterials;
            ViewBag.IngredientId = ingredientId; // Передаем значение ingredientId в представление

                

            return View(ingredient);
        }

        // POST: Обновить ингредиент
        [HttpPost]
        public IActionResult EditIngredient(int ingredientId, int newRawMaterialId, float quantity, int productId)
        {
            // Вызываем хранимую процедуру EditIngredient для обновления ингредиента
            var rowsAffected = _dbContext.Database.ExecuteSqlInterpolated($"EXEC EditIngredient {ingredientId}, {newRawMaterialId}, {quantity}, {productId}");

            if (rowsAffected == -1)
            {
                TempData["ErrorMessage"] = "Another ingredient with the same product and raw material already exists.";
                // Перенаправляем пользователя на GET метод для отображения формы редактирования с сообщением об ошибке и передаем введенные значения
                return RedirectToAction("EditIngredient", new { ingredientId = ingredientId });
            }

            // Иначе, если ингредиент успешно обновлен, перенаправляем пользователя на Index
            return RedirectToAction("Index", "KURSOVAYAIngredient", new { productId = productId });
        }

        [HttpPost]
        public IActionResult DeleteIngredient(int ingredientId, int productId)
        {
            // Вызываем хранимую процедуру DeleteIngredient для удаления ингредиента
            _dbContext.Database.ExecuteSqlInterpolated($"EXEC DeleteIngredient {ingredientId}");

            
            // Перенаправляем пользователя на Index с передачей идентификатора выбранного продукта
            return RedirectToAction("Index", "KURSOVAYAIngredient", new { productId = productId });
        }


    }
}
