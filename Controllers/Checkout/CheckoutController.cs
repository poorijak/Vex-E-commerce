using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers.Checkout
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext db,
            UserManager<Customer> userManager,
            ILogger<CheckoutController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutVm
            {
                CartId = cart.Id,
                Items = cart.Items.Select(i => new CheckoutItemVm
                {
                    CartItemId = i.Id,
                    ProductTitle = i.ProductTitle,
                    VariantText = $"{i.ProductVariant.Color} / {i.ProductVariant.Size}",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            vm.ItemsTotal = vm.Items.Sum(x => x.LineTotal);
            vm.ShippingFee = 40m;
            vm.GrandTotal = vm.ItemsTotal + vm.ShippingFee;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutVm vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Checkout POST: Unauthorized access (no user).");
                return Unauthorized();
            }

            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.Id == vm.CartId && c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Checkout POST: Cart not found or empty for user {UserId}, CartId {CartId}",
                    user.Id, vm.CartId);
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Checkout POST: ModelState invalid for user {UserId}, CartId {CartId}",
                    user.Id, cart.Id);

                vm.Items = cart.Items.Select(i => new CheckoutItemVm
                {
                    CartItemId = i.Id,
                    ProductTitle = i.ProductTitle,
                    VariantText = $"{i.ProductVariant.Color} / {i.ProductVariant.Size}",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                vm.ItemsTotal = vm.Items.Sum(x => x.LineTotal);
                vm.ShippingFee = 40m;
                vm.GrandTotal = vm.ItemsTotal + vm.ShippingFee;

                return View(vm);
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Checkout started for user {UserId}, CartId {CartId}",
                    user.Id, cart.Id);

                // 1) Map จาก CheckoutAddressVm → Address entity
                var address = new Address
                {
                    Name = vm.Address.Name,
                    Phone = vm.Address.Phone,
                    AddressDetail = vm.Address.AddressDetail,
                    Province = vm.Address.Province,
                    PostalCode = vm.Address.PostalCode,
                    Note = vm.Address.Note,
                    CustomerId = user.Id
                };

                _db.addresses.Add(address);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Address created with Id {AddressId} for user {UserId}",
                    address.Id, user.Id);

                // 2) คำนวณยอด
                decimal itemsTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
                decimal shippingFee = 40m;
                decimal grandTotal = itemsTotal + shippingFee;

                _logger.LogInformation(
                    "Calculated totals for CartId {CartId} | ItemsTotal={ItemsTotal}, Shipping={Shipping}, GrandTotal={GrandTotal}",
                    cart.Id, itemsTotal, shippingFee, grandTotal
                );

                // 3) สร้าง Order
                var order = new Order
                {
                    OrderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    totalAmount = grandTotal,
                    shippingFee = shippingFee,
                    status = OrderStatus.pending,
                    createdAt = DateTime.UtcNow,
                    AddressId = address.Id,
                    customerId = user.Id
                };

                _db.orders.Add(order);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Order created: OrderId {OrderId}, OrderNumber {OrderNumber}, UserId {UserId}",
                    order.Id, order.OrderNumber, user.Id);

                // 4) OrderItem + stock
                foreach (var cartItem in cart.Items)
                {
                    var variant = cartItem.ProductVariant;
                    var product = variant.Product;

                    if (variant.Stock < cartItem.Quantity)
                    {
                        _logger.LogWarning(
                            "Insufficient stock for ProductVariant {VariantId} (Product {ProductId}). Stock={Stock}, Requested={Requested}",
                            variant.Id, product.Id, variant.Stock, cartItem.Quantity
                        );

                        ModelState.AddModelError("", $"Stock ไม่พอสำหรับ {cartItem.ProductTitle}");
                        throw new Exception("Insufficient stock");
                    }

                    variant.Stock -= cartItem.Quantity;
                    variant.Sold += cartItem.Quantity;
                    product.TotalStock -= cartItem.Quantity;
                    product.TotalSold += cartItem.Quantity;

                    _logger.LogInformation(
                        "Updated stock/sold for ProductVariant {VariantId} | NewStock={NewStock}, NewSold={NewSold}",
                        variant.Id, variant.Stock, variant.Sold
                    );

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        VariantId = variant.Id,
                        quantity = cartItem.Quantity,
                        price = cartItem.UnitPrice,
                        totalPrice = cartItem.UnitPrice * cartItem.Quantity
                    };

                    _db.orderItems.Add(orderItem);

                    _logger.LogInformation(
                        "OrderItem created: OrderId {OrderId}, VariantId {VariantId}, Qty={Qty}, LineTotal={LineTotal}",
                        order.Id, variant.Id, cartItem.Quantity, orderItem.totalPrice
                    );
                }

                // 5) ลบ cart
                _logger.LogInformation(
                    "Removing CartItems and Cart for CartId {CartId}, UserId {UserId}",
                    cart.Id, user.Id
                );

                _db.CartItems.RemoveRange(cart.Items);
                _db.Carts.Remove(cart);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "Checkout completed successfully for user {UserId}, OrderId {OrderId}",
                    user.Id, order.Id
                );

                TempData["OrderSuccess"] = "สั่งซื้อสำเร็จแล้ว ขอบคุณที่อุดหนุนครับ 🖤";
                return RedirectToAction("Index", "myOrder", new { id = order.Id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Checkout failed for user {UserId}, CartId {CartId}",
                    user?.Id,
                    vm.CartId
                );

                ModelState.AddModelError("", "ไม่สามารถสร้างออเดอร์ได้ กรุณาลองใหม่อีกครั้ง");
                return View(vm);
            }
        }
    }
}
