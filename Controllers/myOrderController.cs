using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Vex_E_commerce.Controllers.Checkout;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers
{
    public class myOrderController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<CheckoutController> _logger;
        private readonly IConfiguration _config;

        public myOrderController(ApplicationDbContext db, UserManager<Customer> userManager, ILogger<CheckoutController> logger  , IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }

        public async Task<IActionResult> Index(Guid id)
        {

            var user = await _userManager.GetUserAsync(User);

            var order = await _db.orders
    .Include(o => o.Items)
        .ThenInclude(oi => oi.variant)
        .ThenInclude(v => v.Product)
    .Include(o => o.address)
    .Include(o => o.customer)
    .FirstOrDefaultAsync(o => o.Id == id && o.customerId == user.Id);

            var vm = new OrderDetailVm
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.status,
                TotalAmount = order.totalAmount + 2,
                ShippingFee = order.shippingFee,
                CreatedAt = order.createdAt,
                PaymentAt = order.paymentAt,
                TrackingNumber = order.trackingNumver,
                Tax = 2,

                CustomerId = order.customerId,
                CustomerName = order.customer.Name,
                CustomerEmail = order.customer.Email,

                Address = new OrderAddressVm
                {
                    Name = order.address.Name,
                    Phone = order.address.Phone,
                    AddressDetail = order.address.AddressDetail,
                    Province = order.address.Province,
                    PostalCode = order.address.PostalCode,
                    Note = order.address.Note
                },

                Items = order.Items.Select(oi => new OrderItemVm
                {
                    PictureUrl = oi.variant.Product.PictureUrl,
                    OrderItemId = oi.Id,
                    ProductTitle = oi.variant.ProductTitle,
                    VariantText = $"{oi.variant.Color} / {oi.variant.Size}",
                    Quantity = oi.quantity,
                    UnitPrice = oi.price,
                }).ToList()
            };

            vm.ItemsTotal = vm.Items.Sum(i => i.LineTotal);

            // ถ้ามีรูปอยู่แล้ว แปลงเป็น base64 ให้ View ใช้ <img>
            if (order.paymentImage != null)
            {
                vm.PaymentImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(order.paymentImage)}";
            }

            var promptPayPhone = _config["PromptPay:PhoneNumber"];

            ViewBag.Phone = promptPayPhone;

            return View(vm);
        }
    }
}
