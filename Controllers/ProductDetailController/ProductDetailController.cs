using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class ProductDetailController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;

        public ProductDetailController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(Guid? id)
        {
            var userId = _userManager.GetUserId(User);

            HashSet<Guid> wishedIds = new();
            if (userId != null)
            {
                wishedIds = (await _context.ProductWishlists
                    .AsNoTracking()
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync()).ToHashSet();
            }

            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);


            var relateProduct = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id != product.Id && p.Status == ProductStatus.Active)
            .Select(p => new ProductCardVm
            {
            Id = p.Id,
            Title = p.Title,
            PictureUrl = p.PictureUrl,
            TotalSold = p.TotalSold,
            BasePrice = p.BasePrice,
            IsWishlisted = wishedIds.Contains(p.Id)
            })
            .Take(5)
            .ToListAsync();


            if (product == null)
                return NotFound();

            var sizeStock = product.ProductVariants
                       .GroupBy(v => v.Size)
                       .ToDictionary(g => g.Key.ToString(), g => g.Sum(v => v.Stock));

            ViewBag.SizeStock = sizeStock;
            ViewBag.categoryTitle = product.Category.Title;


            ViewBag.VariantsJson = System.Text.Json.JsonSerializer.Serialize(
            product.ProductVariants.Select(v => new
            {
                color = v.Color.ToString(),
                size = v.Size.ToString(),
                stock = v.Stock
            })
            );

            var vm = new ProductDetailDTO
            {
                Product = product,
                RelateProducts = relateProduct
            };

            return View(vm);
        }

        public async Task<IActionResult> ToggleWishList([FromForm] Guid productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var exist = await _context.ProductWishlists.FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

            if (exist == null)
            {
                await _context.ProductWishlists.AddAsync(new ProductWishlist { UserId = user.Id, ProductId = productId });
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.ProductWishlists.Remove(exist);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("index", "ProductDetail" , new { id = productId});
        }



    }
}




