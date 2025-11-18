using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vex_E_commerce.Data;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers.Cartpage
{
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;

        public CartController(ApplicationDbContext db, UserManager<Customer> userManager)
        {
            _db = db; _userManager = userManager;
        }
                                                                                                                                            
        // POST /cart/add
        [HttpPost("add")]
        [ValidateAntiForgeryToken]                       
        public async Task<IActionResult> Add(AddToCartDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // 1) หา variant จากสิ่งที่ user เลือก
            var variant = await _db.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v =>
                    v.ProductId == dto.ProductId &&
                    v.Color == dto.Color &&
                    v.Size == dto.Size);

            if (variant == null)
                return BadRequest("Variant not found");

            if (variant.Stock <= 0 || variant.Stock < dto.Qty)
                return BadRequest("Out of stock");

            // 2) หา/สร้าง Cart ของ user
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            // 3) ถ้ามี CartItem ของ variant เดิมอยู่แล้ว → บวกจำนวน
            var item = cart.Items.FirstOrDefault(i => i.ProductVariantId == variant.Id);

            if (item == null)
            {
                item = new CartItem
                {
                    Cart = cart,
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,

                    // ให้ตรง model
                    Count = dto.Qty,
                    Quantity = dto.Qty,
                    UnitPrice = variant.BasePrice,
                    Price = variant.BasePrice,
                    ProductTitle = variant.Product.Title
                };

                cart.Items.Add(item);
            }
            else
            {
                item.Count += dto.Qty;
                item.Quantity += dto.Qty;
            }

            await _db.SaveChangesAsync();
            return Json(new { ok = true, cartId = cart.Id, itemId = item.Id });
        }
        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewCart(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (cart == null)
                return NotFound("Cart not found");

            return View("Index",cart); // ส่ง cart ไปหน้า View
        }
    }

    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public ProductColor Color { get; set; }   // ใช้ enum ของคุณ
        public ProductSize Size { get; set; }
        public int Qty { get; set; } = 1;
    }

                        //ใช้ var = shipping fee 40 แล้วบวกราคา -> view.back ใน index
    //เอา sum มา ทำใน model 
    //    ใช้Java reduce อะไรสักอย่าง

}
