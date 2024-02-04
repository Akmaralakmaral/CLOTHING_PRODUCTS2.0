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

        public async Task<IActionResult> Index(int? productId)
        {
            var products = await _dbContext.FinishedProducts.ToListAsync();
            ViewData["Products"] = new SelectList(products, "FinishedProductId", "Name");

            if (productId.HasValue)
            {
                ViewBag.SelectedProductId = productId;

                var productDetails = await _dbContext.Ingredients
                    .Where(e => e.FinishedProductId == productId)
                    .Include(e => e.RawMaterial)
                    .Include(e => e.FinishedProduct)
                    .ToListAsync();

                ViewBag.ProductDetails = productDetails;

            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
            var productDetails = await _dbContext.Ingredients
                .Where(e => e.FinishedProductId == productId)
                .Include(e => e.RawMaterial)
                .Include(e => e.FinishedProduct)
                .ToListAsync();

            return PartialView("_ProductDetails", productDetails);
        }



        public IActionResult Create(int selectedProductId)
        {
            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
            var finishedProducts = _dbContext.FinishedProducts.ToList();
            ViewBag.FinishedProducts = new SelectList(finishedProducts, "FinishedProductId", "Name", selectedProductId);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            if (_dbContext.Ingredients.Any(i => i.FinishedProductId == ingredient.FinishedProductId && i.RawMaterialId == ingredient.RawMaterialId))
            {
                ModelState.AddModelError("RawMaterialId", "This Raw Material is already assigned to the selected Finished Product.");
                ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                var finishedProducts = _dbContext.FinishedProducts.ToList();
                ViewBag.FinishedProducts = new SelectList(finishedProducts, "FinishedProductId", "Name", ingredient.FinishedProductId);
                return View(ingredient);
            }

            _dbContext.Ingredients.Add(ingredient);
            await _dbContext.SaveChangesAsync();

            // Redirect to the Index action with the FinishedProductId
            //return RedirectToAction(nameof(Index), new { productId = ingredient.FinishedProductId });
            // Redirect to the Index action with the FinishedProductId of the new ingredient
            return RedirectToAction(nameof(Index), new { productId = ingredient.FinishedProductId });
        }







        public IActionResult EditIngredient(int id)
        {
            var ingredient = _dbContext.Ingredients
                .Include(i => i.RawMaterial)
                .Include(i => i.FinishedProduct) // Включаем завершенный продукт
                .FirstOrDefault(i => i.IngredientId == id);

            if (ingredient == null)
            {
                return NotFound();
            }

            ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIngredient(Ingredient ingredient)
        {

            try
            {

                //if (_dbContext.Ingredients.Any(i => i.FinishedProductId == ingredient.FinishedProductId && i.RawMaterialId == ingredient.RawMaterialId))
                //{
                //    ModelState.AddModelError("RawMaterialId", "This Raw Material is already assigned to another Finished Product.");
                //    // Повторно загружаем список сырья для представления
                //    ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                //    return View(ingredient);
                //}
                if (_dbContext.Ingredients.Any(i => i.FinishedProductId == ingredient.FinishedProductId && i.RawMaterialId == ingredient.RawMaterialId && i.IngredientId != ingredient.IngredientId))
                {
                    ModelState.AddModelError("RawMaterialId", "This Raw Material is already assigned to another Finished Product.");
                    // Повторно загружаем список сырья для представления
                    ViewBag.RawMaterials = _dbContext.RawMaterials.ToList();
                    return View(ingredient);
                }

                // Если сырье не найдено в других продуктах, обновляем состояние ингредиента
                _dbContext.Entry(ingredient).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                /* return RedirectToAction(nameof(Index));*/ // Перенаправление на метод Index
                return RedirectToAction(nameof(Index), new { productId = ingredient.FinishedProductId });

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IngredientExists(ingredient.IngredientId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        private bool IngredientExists(int id)
        {
            return _dbContext.Ingredients.Any(e => e.IngredientId == id);
        }


        //[HttpPost]
        //public IActionResult DeleteIngredient(int ingredientId)
        //{
        //    var ingredient = _dbContext.Ingredients.Find(ingredientId);

        //    if (ingredient != null)
        //    {
        //        _dbContext.Ingredients.Remove(ingredient);
        //        _dbContext.SaveChanges();
        //        return Json(new { success = true });
        //    }

        //    return Json(new { success = false });
        //}


        [HttpPost]
        public IActionResult DeleteIngredient(int ingredientId)
        {
            var ingredient = _dbContext.Ingredients.Find(ingredientId);

            if (ingredient != null)
            {
                // Получаем productId перед удалением ингредиента
                int productId = ingredient.FinishedProductId;

                _dbContext.Ingredients.Remove(ingredient);
                _dbContext.SaveChanges();

                // После успешного удаления, выполните перенаправление на метод Index с указанным productId
                return Json(new { success = true, productId = productId });
            }

            return Json(new { success = false });
        }



    }
}
