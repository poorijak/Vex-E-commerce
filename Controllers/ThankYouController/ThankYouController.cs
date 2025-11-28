using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class ThankYouController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<Customer> _userManager;

        public ThankYouController(ApplicationDbContext context, UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /ThankYou?cartId={id}
        public async Task<IActionResult> Index(Guid id)
        {
            // ดึงข้อมูลตะกร้า
            var order = await _context.orders.FirstOrDefaultAsync(o => o.Id == id);

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


            var relateProduct = await _context.Products
            .AsNoTracking()
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
            if (order == null)
            {
                return NotFound();
            }

            var vm = new ThankyouPageVm
            {
                RelateProduct = relateProduct,
                OrderId = order.OrderNumber
            };
            return View(vm);
        }



    }
}
