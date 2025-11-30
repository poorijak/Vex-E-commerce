using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // แสดงหมวดหมู่ทั้งหมด
        public IActionResult Index()
        {
            var categories = _context.Categories.Include(c => c.products).ToList();

            var viewModel = new CategoryPageViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        public IActionResult Detail(Guid id)
        {
            var category = _context.Categories
                .Include(c => c.products)
                    .ThenInclude(p => p.ProductVariants)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            var viewModel = new CategoryDetailViewModel
            {
                Category = category,
                Products = category.products.ToList(),
                ProductVariants = category.products 
                    .SelectMany(p => p.ProductVariants)
                    .ToList()
            };

            return View(viewModel);

        }

    }
}
