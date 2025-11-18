using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class ProductDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(Guid? id)
        {

            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductVariants)                              
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var sizeStock = product.ProductVariants
                       .GroupBy(v => v.Size)
                       .ToDictionary(g => g.Key.ToString(), g => g.Sum(v => v.Stock));

            ViewBag.SizeStock = sizeStock;



            return View(product); 
        }

        [HttpGet]
        public IActionResult GetVariantsBySize(Guid productId, string color, string size)
        {
            ProductColor colorEnum = Enum.Parse<ProductColor>(color);
            ProductSize sizeEnum = Enum.Parse<ProductSize>(size);

            var variant = _context.ProductVariants
                .Where(v => v.ProductId == productId
                            && v.Color == colorEnum
                            && v.Size == sizeEnum)
                .FirstOrDefault(); // ตัวเดียว

            return PartialView("_VariantListPartial", variant); // ส่งตัวเดียว
        }






    }
}




