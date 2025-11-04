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

        var product = await _db.Products.Take(5).ToListAsync();

        return View(product);
    }
    public async Task<IActionResult> ProductList()
    {

        var product = await _db.Products.Take(5).ToListAsync();

        return View(product);
    }

    public async Task<IActionResult> CategoryProduct(string category)
    {

        var prodcutList = await _db.Products.Where(p => p.Category.Title == category).ToListAsync();

        ViewBag.Category = category;

        return View(prodcutList);
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
            return Json(new { ok = true, addred = false });
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
