using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public async Task<IActionResult> CreateProduct()
        {
            var vm = new ProductFormVm
            {
                Categories = await _db.Categories
                    .OrderBy(c => c.Title)
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _db.Categories
                    .OrderBy(c => c.Title)
                    .ToListAsync();
                return View(vm);
            }

            var newProduct = new Product
            {
                Title = vm.Title,
                Description = vm.Description,
                BasePrice = vm.BasePrice ?? 0,
                CategoryId = vm.CategoryId,
                PictureUrl = vm.PhotoUrl,
                Status = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // ถ้าคุณยังใช้ VariantJson สำหรับหน้า Create
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
                                    ProductTitle = newProduct.Title,
                                    Color = color,
                                    Size = size,
                                    BasePrice = data.Price,
                                    Stock = data.Stock,
                                    Sku = data.Sku,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                        newProduct.TotalStock = newProduct.ProductVariants.Sum(v => v.Stock);
                    }
                }
                catch (JsonException)
                {
                    ModelState.AddModelError("VariantJson", "ข้อมูล Variant ไม่ถูกต้อง.");
                    vm.Categories = await _db.Categories
                        .OrderBy(c => c.Title)
                        .ToListAsync();
                    return View(vm);
                }
            }

            _db.Products.Add(newProduct);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product '{newProduct.Title}' created successfully!";
            return RedirectToAction("Index", "Admin");
        }

        // ========== EDIT ==========
        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid? id)
        {
            if (id == null) return NotFound();

            var p = await _db.Products
                .Include(x => x.ProductVariants)
                .FirstOrDefaultAsync(x => x.Id == id.Value);
            if (p == null) return NotFound();

            var categories = await _db.Categories
                .OrderBy(c => c.Title)
                .ToListAsync();

            var vm = new ProductFormVm
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                BasePrice = p.BasePrice,
                CategoryId = p.CategoryId,
                Categories = categories,
                PhotoUrl = p.PictureUrl,

                Variants = p.ProductVariants.Select(v => new TemporaryVariantData
                {
                    Size = v.Size.ToString(),
                    Color = v.Color.ToString(),
                    Price = v.BasePrice,
                    Stock = v.Stock,
                    Sku = v.Sku
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _db.Categories
                    .OrderBy(c => c.Title)
                    .ToListAsync();

                return View(vm);
            }

            var p = await _db.Products
                .Include(x => x.ProductVariants)
                .FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (p == null) return NotFound();

            p.Title = vm.Title;
            p.Description = vm.Description;
            p.BasePrice = vm.BasePrice ?? 0;
            p.CategoryId = vm.CategoryId;
            p.PictureUrl = vm.PhotoUrl;

            var existing = await _db.ProductVariants
                .Where(v => v.ProductId == p.Id)
                .ToListAsync();

            _db.ProductVariants.RemoveRange(existing);

            // เพิ่ม variants ใหม่
            foreach (var x in vm.Variants ?? new())
            {
                if (Enum.TryParse(x.Color, true, out ProductColor color) &&
                    Enum.TryParse(x.Size, true, out ProductSize size))
                {
                    _db.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = p.Id,     // ใส่ FK ให้ชัด
                        ProductTitle = p.Title,
                        Color = color,
                        Size = size,
                        BasePrice = x.Price,
                        Stock = x.Stock,
                        Sku = x.Sku,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            p.TotalStock = (vm.Variants ?? new()).Sum(v => v.Stock);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // ตรวจอีกชั้นว่าสินค้ายังอยู่ไหม
                var exists = await _db.Products.AnyAsync(x => x.Id == vm.Id);
                if (!exists) return NotFound();

                vm.Categories = await _db.Categories
                    .OrderBy(c => c.Title)
                    .ToListAsync();
                ModelState.AddModelError(string.Empty, "ข้อมูลถูกแก้ไข/ลบโดยผู้อื่น กรุณาโหลดหน้าใหม่แล้วลองอีกครั้ง");
                return View(vm);
            }

            return RedirectToAction("Index", "Admin");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
