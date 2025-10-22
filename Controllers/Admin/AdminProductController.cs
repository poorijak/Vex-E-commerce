using Microsoft.AspNetCore.Mvc;

namespace Vex_E_commerce.Controllers.Admin
{
    public class AdminProductController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(CreateProduct));
        }
        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }
    }
}
