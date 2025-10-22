using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers.Admin
{

    public class AdminController : Controller
    {
        private readonly UserManager<Customer> _userManager;


        public AdminController(UserManager<Customer> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var role = user.Role;

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
    }
}
