using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Diagnostics;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<Customer> _userManager;




    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<Customer> userManager)
    {
        _logger = logger;
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
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

        var lists = await _db.Products.AsNoTracking().Select(p => new ProductCardVm
        {
            Id = p.Id,
            Title = p.Title,
            PictureUrl = p.PictureUrl,
            TotalSold = p.TotalSold,
            BasePrice = p.BasePrice,
            IsWishlisted = wishedIds.Contains(p.Id),

        }).Take(5).ToListAsync();


        return View(lists);
    }

    public async Task<IActionResult> CategoryProduct(string category , string? sort)
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
         .Where(p => p.Category.Title == category) 
         .Select(p => new ProductCardVm
         {
             Id = p.Id,
             Title = p.Title,
             PictureUrl = p.PictureUrl,
             TotalSold = p.TotalSold,
             BasePrice = p.BasePrice,
             IsWishlisted = wishedIds.Contains(p.Id),
         });


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

        var model = await q.ToListAsync();




        ViewBag.Sort = sort;
        ViewBag.Category = category;

        return View(model);
    }

    public async Task<IActionResult> ToggleWishList([FromForm] Guid productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var exist = await _db.ProductWishlists.FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

        if (exist == null)
        {
            await _db.ProductWishlists.AddAsync(new ProductWishlist { UserId = user.Id, ProductId = productId });
            await _db.SaveChangesAsync();
            return Json(new { ok = true, added = true });
        }
        else
        {
            _db.ProductWishlists.Remove(exist);
            await _db.SaveChangesAsync();
            return Json(new { ok = true, added = false });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
