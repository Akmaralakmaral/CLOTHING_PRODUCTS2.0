using CLOTHING_PRODUCTS.Context;
using CLOTHING_PRODUCTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    [Route("Ingredient")]
    public class IngredientController : Controller
    {
        private readonly AddDBContext _dbContext;

        public IngredientController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var finishedProducts = _dbContext.FinishedProducts.ToList();
            ViewBag.FinishedProducts = finishedProducts;
            return View();
        }

        [Route("GetIngredients/{finishedProductId}")]
        public IActionResult GetIngredients(int finishedProductId)
        {
            var ingredients = _dbContext.Ingredients
                .Where(i => i.FinishedProductId == finishedProductId)
                .Include(i => i.RawMaterial)
                .Select(i => new
                {
                    RawMaterialName = i.RawMaterial.Name,
                    Quantity = i.Quantity
                })
                .ToList();

            return Json(ingredients);
        }

    }
}
