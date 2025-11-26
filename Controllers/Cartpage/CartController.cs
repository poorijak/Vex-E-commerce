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

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

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

            // --- คำนวณราคารวม ---
            decimal itemsTotal = cart.Items.Sum(i => i.Price * i.Quantity); // รวมราคาสินค้า
            decimal shippingFee = 40m; // ค่าจัดส่ง
            decimal tax = 2m; // ภาษี

            decimal totalAmount = itemsTotal + shippingFee + tax;

            cart.CartTotal = totalAmount;



            // ส่งข้อมูลไป View
            ViewBag.ItemsTotal = itemsTotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Tax = tax;
            ViewBag.TotalAmount = totalAmount;

            await _db.SaveChangesAsync();
            return View("Index", cart); // ส่ง cart ไปหน้า View
        }

        // POST /cart/delete
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid itemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItem = await _db.CartItems
                .Include(i => i.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == user.Id);

            if (cartItem == null) return NotFound();

            var cart = cartItem.Cart;

            // ลบ item ก่อน
            _db.CartItems.Remove(cartItem);

            // ถ้าใน cart มี item แค่ 1 ชิ้น แปลว่าพอลบเสร็จจะเหลือ 0 → ลบ cart ทิ้ง
            var cartWillBeEmpty = cart.Items.Count == 1;

            if (cartWillBeEmpty)
            {
                _db.Carts.Remove(cart);
            }

            await _db.SaveChangesAsync();

            if (cartWillBeEmpty)
            {
                // ตะกร้าถูกลบแล้ว → กลับหน้า Home
                return RedirectToAction("Index", "Home");
            }

            // ยังมีของในตะกร้า → อยู่หน้า ViewCart ต่อ
            return RedirectToAction("ViewCart", new { id = cart.Id });
        }


        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            decimal itemsTotal = cart.Items.Sum(i => i.Price * i.Quantity);
            decimal shippingFee = 40m;
            decimal tax = 2m;

            ViewBag.ItemsTotal = itemsTotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Tax = tax;
            ViewBag.TotalAmount = itemsTotal + shippingFee + tax;

            return View("Index", cart);
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
