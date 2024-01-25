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

        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.FinishedProducts.ToListAsync();
            ViewData["Products"] = new SelectList(products, "FinishedProductId", "Name");
            return View();

        }

        [HttpGet]
        public IActionResult GetProductDetails(int productId)
        {
            // Retrieve the details for the selected product and pass it to the partial view
            var productDetails = _dbContext.Ingredients
                .Where(e => e.FinishedProductId == productId)
                .Include(e => e.RawMaterial)
                .Include(e => e.FinishedProduct)
                .ToList();

            return PartialView("_ProductDetails", productDetails);
        }

        public IActionResult Create(int? selectedFinishedProductId)
        {
            ViewBag.FinishedProducts = _dbContext.FinishedProducts.ToList();
            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();

            // Set the selected value for FinishedProductId in ViewBag
            ViewBag.SelectedFinishedProductId = selectedFinishedProductId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            // Check if an ingredient with the same FinishedProductId and RawMaterialId already exists
            if (_dbContext.Ingredients.Any(i => i.FinishedProductId == ingredient.FinishedProductId && i.RawMaterialId == ingredient.RawMaterialId))
            {
                ModelState.AddModelError("RawMaterialId", "This Raw Material is already assigned to the selected Finished Product.");
                ViewBag.FinishedProducts = _dbContext.FinishedProducts.ToList();
                ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                return View(ingredient);
            }

            // If validation passes, continue with the creation of the ingredient
            _dbContext.Ingredients.Add(ingredient);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        public IActionResult EditQuantity(int ingredientId, int newQuantity)
        {
            var ingredient = _dbContext.Ingredients.Find(ingredientId);

            if (ingredient != null)
            {
                ingredient.Quantity = newQuantity;
                _dbContext.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult DeleteIngredient(int ingredientId)
        {
            var ingredient = _dbContext.Ingredients.Find(ingredientId);

            if (ingredient != null)
            {
                _dbContext.Ingredients.Remove(ingredient);
                _dbContext.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }



    }
}
