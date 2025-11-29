using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    [Authorize] // ต้องล็อกอิน
    public class MyOrder : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ApplicationDbContext _db;

        public MyOrder(UserManager<Customer> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index(string status = "pending")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // แปลง string → Enum
            Enum.TryParse(status, true, out OrderStatus parsedStatus);

            ViewBag.Status = parsedStatus.ToString().ToLower();

            var orders = await _db.orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.variant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.address)
                .Where(o => o.customerId == user.Id && o.status == parsedStatus)
                .OrderByDescending(o => o.createdAt)
                .ToListAsync();

            return View(orders);
        }
    }
}
