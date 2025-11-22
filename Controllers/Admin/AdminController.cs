using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.Drawing.Printing;
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

        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 5)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.Role.ToString() == "Admin")
            {

                IQueryable<Product> q = _db.Products.AsNoTracking().Where(p => p.Status == ProductStatus.Active);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var keywordTrim = keyword.Trim();

                    q = q.Where(p => p.Title.Contains(keywordTrim));
                }




                var total = await q.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);

                var products = await q
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.keyword = keyword;
                ViewBag.Page = page;
                ViewBag.TotalPages = totalPages;

                return View(products);
            }
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProduct(Guid? id)
        {
            var product = await _db.Products.FindAsync(id);

            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Admin");
        }

        public async Task<IActionResult> Orders()
        {
            return View();
        }
        public async Task<IActionResult> OrderDetail()
        {
            return View();
        }

        public async Task<IActionResult> Customer(int page = 1, int pageSize = 5)
        {


            var Customers = await _userManager.Users
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();


            var total = await _userManager.Users.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(Customers);
        }

        public async Task<IActionResult> CustomerDetail(string id)
        {

            if (string.IsNullOrWhiteSpace(id)) return BadRequest();


            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();


            var vm = new CustomerVm
            {
                User = user,
                Form = new CustomerFormDto
                {
                    Role = user.Role,
                    Status = user.Status,
                    Id = user.Id
                }

            };

            ViewBag.RoleList = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString(),
                    Selected = (r == user.Role)
                }).ToList();

            ViewBag.StatusList = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = s.ToString(),
                    Selected = (s == user.Status)
                }).ToList();


            if (vm.User is null)
            {
                return NotFound();
            }

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerDetail(CustomerVm vm)
        {
            var user = await _userManager.FindByIdAsync(vm.Form.Id);
            if (user == null) return NotFound();

            if (vm.Form.Status == user.Status)
                ModelState.AddModelError("Form.Status", "Existing Status");

            if (vm.Form.Role == user.Role)
                ModelState.AddModelError("Form.Role", "Existing Role");

            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            // 3) อัปเดตจริง
            user.Role = vm.Form.Role;
            user.Status = vm.Form.Status;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                ModelState.AddModelError("", errors);

                // เติม dropdown กลับก่อน return
                ViewBag.RoleList = Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() })
                    .ToList();

                ViewBag.StatusList = Enum.GetValues(typeof(UserStatus))
                    .Cast<UserStatus>()
                    .Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() })
                    .ToList();

                vm.User ??= user;
                return View(vm);
            }

            return RedirectToAction("Customer", "Admin");
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
