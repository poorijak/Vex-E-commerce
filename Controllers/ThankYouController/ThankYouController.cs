using Microsoft.AspNetCore.Mvc;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Vex_E_commerce.Controllers
{
    public class ThankYouController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThankYouController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ThankYou?cartId={id}
        public async Task<IActionResult> Index(Guid cartId)
        {
            // ดึงข้อมูลตะกร้า
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }
    }
}
