using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.CodeDom;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers.Admin
{

    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly UserManager<Customer> _userManager;
        private readonly ApplicationDbContext _db;


        public AdminController(UserManager<Customer> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.Role.ToString() == "Admin")
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Orders()
        {
            return View();
        }

        public async Task<IActionResult> Customer()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Category()
        {

            var vm = new CategoryPageViewModel
            {
                Category = new Category(),
                Categories = await _db.Categories.ToListAsync()
            };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryPageViewModel vm)
        {
            var existingCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.Title == vm.Category.Title);

            if (existingCategory != null)
            {
                ModelState.AddModelError("Title", "A category with this title already exists.");
                return View("Category", vm);
            }

            if (!ModelState.IsValid)
            {
                return View("Category", vm);
            }



            await _db.Categories.AddAsync(vm.Category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Category));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {

            var result = await _db.Categories.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(result);
            _db.SaveChanges();
            return RedirectToAction(nameof(Category));

        }
    }


}
