using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.CodeDom;
using Vex_E_commerce.Data;

namespace Vex_E_commerce.Controllers.Admin
{
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _db;


        public AdminProductController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(CreateProduct));
        }
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var cate = await _db.Categories.ToListAsync();
            return View(cate);
        }
    }
}
