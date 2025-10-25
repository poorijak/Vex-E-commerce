using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;
using Vex_E_commerce.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vex_E_commerce.Controllers.Admin
{
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public AdminProductController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var vm = new ProductCreateViewModel
            {
                Categories = await _db.Categories.ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _db.Categories.ToListAsync();
                return View(vm);
            }

            string? pictureUrl = vm.PhotoUrl;

            var newProduct = new Product
            {
                Title = vm.Title,
                Description = vm.Description,
                BasePrice = vm.BasePrice ?? 0,
                CategoryId = vm.CategoryId,
                PictureUrl = pictureUrl,
                Status = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(vm.VariantJson))
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var variantData = JsonSerializer.Deserialize<List<TemporaryVariantData>>(vm.VariantJson, jsonOptions);

                    if (variantData != null)
                    {
                        foreach (var data in variantData)
                        {
                            if (Enum.TryParse(data.Color, true, out ProductColor color) &&
                                Enum.TryParse(data.Size, true, out ProductSize size))
                            {
                                newProduct.ProductVariants.Add(new ProductVariant
                                {
                                    Color = color,
                                    Size = size,
                                    BasePrice = (decimal)data.Price,
                                    Stock = data.Stock,
                                    Sku = data.Sku,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    ModelState.AddModelError("VariantJson", "ข้อมูล Variant ไม่ถูกต้อง.");
                    vm.Categories = await _db.Categories.ToListAsync();
                    return View(vm);
                }
            }

            _db.Products.Add(newProduct);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product '{newProduct.Title}' created successfully!";
            return RedirectToAction("Index" , "Admin");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}