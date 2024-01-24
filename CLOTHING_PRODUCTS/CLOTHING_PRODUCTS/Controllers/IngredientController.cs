using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class IngredientController : Controller
    {
        private readonly AddDBContext _dbContext;

        public IngredientController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            // Получаем список видов готовой продукции
            var finishedProducts = _dbContext.FinishedProducts.ToList();
            ViewBag.FinishedProducts = new SelectList(finishedProducts, "FinishedProductId", "Name");

            return View(new List<Ingredient>()); // Пустой список, чтобы избежать ошибки
        }

        [HttpPost]
        public IActionResult GetIngredients(int finishedProductId)
        {
            // Получаем список ингредиентов для выбранного продукта
            var ingredients = _dbContext.Ingredients
                .Where(i => i.FinishedProductId == finishedProductId)
                .Include(i => i.RawMaterial)
                .ToList();

            return PartialView("_IngredientsPartial", ingredients);
        }
    }
}
