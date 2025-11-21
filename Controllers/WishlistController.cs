﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    [Authorize] // ต้องล็อกอินก่อนถึงเข้าได้
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;

        public WishlistController(ApplicationDbContext db, UserManager<Customer> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId is null)
            {
                return Challenge(); // เด้งไปหน้า login
            }

            // ดึง product ทั้งหมดที่ user คนนี้กด wishlist
            List<ProductCardVm> model = await _db.ProductWishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .Select(w => new ProductCardVm
                {
                    Id = w.Product.Id,
                    Title = w.Product.Title,
                    PictureUrl = w.Product.PictureUrl ?? string.Empty,
                    TotalSold = w.Product.TotalSold,
                    BasePrice = w.Product.BasePrice,
                    IsWishlisted = true
                })
                .ToListAsync();

            return View(model); // <-- List<ProductCardVm>
        }
    }
}