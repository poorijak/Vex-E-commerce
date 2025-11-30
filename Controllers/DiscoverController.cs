using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class DiscoverController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;

        public DiscoverController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<Customer> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? keyword , string? sort)
        {

            var userId = _userManager.GetUserId(User);

            HashSet<Guid> wishedIds = new();
            if (userId != null)
            {
                wishedIds = (await _db.ProductWishlists
                    .AsNoTracking()
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync()).ToHashSet();
            }

            IQueryable<ProductCardVm> q = _db.Products
            .AsNoTracking()
            .Select(p => new ProductCardVm
            {
                Id = p.Id,
                Title = p.Title,
                PictureUrl = p.PictureUrl,
                TotalSold = p.TotalSold,
                BasePrice = p.BasePrice,
                IsWishlisted = wishedIds.Contains(p.Id),
            });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordTrim = keyword.Trim().ToLower();
                q =  q.Where(p => p.Title.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "priceAsc":
                        q = q.OrderBy(p => p.BasePrice);
                        break;
                    case "priceDsec":
                        q = q.OrderByDescending(p => p.BasePrice);
                        break;
                    default:
                        q = q.OrderBy(p => p.Title);
                        break;
                }
            }


            var model = await  q.ToListAsync();

            ViewBag.sort = sort;
            ViewBag.keyword = keyword;

            return View(model);
        }
    }
}
