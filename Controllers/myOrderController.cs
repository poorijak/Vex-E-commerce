using CatBox.NET;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Vex_E_commerce.Controllers.Checkout;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;
using Vex_E_commerce.Services;

namespace Vex_E_commerce.Controllers
{
    public class myOrderController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<myOrderController> _logger;
        private readonly IConfiguration _config;
        private readonly CatboxServices _catboxServices;

        public myOrderController(ApplicationDbContext db, UserManager<Customer> userManager, ILogger<myOrderController> logger, IConfiguration config, CatboxServices catebox)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _config = config;
            _catboxServices = catebox;
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
                vm.PaymentImageUrl = order.paymentImage;
            }

            var promptPayPhone = _config["PromptPay:PhoneNumber"];

            ViewBag.Phone = promptPayPhone;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSlip(Guid orderId, OrderDetailVm model)
        {
            // 1. ดึง Order เก่ามาจาก DB
            var order = await _db.orders.FindAsync(orderId);
            if (order == null) return NotFound();

            // 2. ตรวจสอบไฟล์และอัปโหลดไป Catbox
            if (model.PaymentFile != null && model.PaymentFile.Length > 0)
            {
                // เรียกใช้ Service ที่สร้างไว้
                string catboxUrl = await _catboxServices.UploadImageAsync(model.PaymentFile);

                order.paymentImage = catboxUrl;


                order.paymentAt = DateTime.UtcNow;
                order.status = OrderStatus.paid; // หรือสถานะรอตรวจสอบ


                _db.orders.Update(order);
                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(order.paymentImage))
            {
                model.PaymentImageUrl = order.paymentImage;
            }

            return RedirectToAction("Index", "ThankYou", new { id = orderId });
        }


    }





}
