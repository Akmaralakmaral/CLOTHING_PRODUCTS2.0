using CLOTHING_PRODUCTS.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLOTHING_PRODUCTS.Controllers
{
    public class RawMaterialController : Controller
    {
        private readonly AddDBContext _dbContext;
        public RawMaterialController(AddDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var rawMaterials = await _dbContext.RawMaterials
                .Include(e => e.MeasurementUnit)
                .ToListAsync();
            return View(rawMaterials);
        }
       
    }
}
