using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.Drawing.Printing;
using Vex_E_commerce.Data;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Controllers.Admin
{

    public class AdminController : Controller
    {

        private readonly UserManager<Customer> _userManager;
        private readonly ApplicationDbContext _db;


        public AdminController(UserManager<Customer> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 5)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.Role.ToString() == "Admin")
            {

                IQueryable<Product> q = _db.Products.AsNoTracking().Where(p => p.Status == ProductStatus.Active);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var keywordTrim = keyword.Trim();

                    q = q.Where(p => p.Title.Contains(keywordTrim) || p.Category.Title.Contains(keywordTrim));
                }




                var total = await q.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);

                var products = await q
                    .OrderBy(p => p.Id)
                    .Include(p => p.Category)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.keyword = keyword;
                ViewBag.Page = page;
                ViewBag.TotalPages = totalPages;

                return View(products);
            }

            ViewBag.keyword = keyword;
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProduct(Guid? id)
        {
            var product = await _db.Products.FindAsync(id);

            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Admin");
        }

        public async Task<IActionResult> Orders(string? keyword ,  int page = 1, int pageSize = 5)
        {
            var ordersQuery = _db.orders
                .Include(o => o.customer)
                .Include(o => o.Items)
                .AsNoTracking();


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordTrim = keyword.Trim();

                ordersQuery = ordersQuery.Where(o => o.OrderNumber.Contains(keywordTrim) || o.customer.Email.Contains(keywordTrim));
            }

            // Pagination
            var total = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var orders = await ordersQuery
                .Include(o => o.customer)
                .OrderByDescending(o => o.createdAt)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(Guid id)
        {

            var order = await _db.orders
                .Include(o => o.customer)
                .Include(o => o.address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var vm = new OrderDetailVm
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.status,
                TotalAmount = order.totalAmount,
                ShippingFee = order.shippingFee,
                CreatedAt = order.createdAt,
                PaymentAt = order.paymentAt,
                TrackingNumber = order.trackingNumver,
                PaymentImageUrl = order.paymentImage,

                CustomerId = order.customer.Id,
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

                Items = order.Items
            .Select(i =>
            {
                var variant = i.variant;
                var product = variant?.Product;
                var category = product?.Category;   // ถ้า Category เป็น entity

                return new OrderItemVm
                {
                    OrderItemId = i.Id,
                    PictureUrl = product?.PictureUrl ?? "",                // fallback เป็น "" ถ้า null
                    ProductTitle = product?.Title ?? "(Unknown product)",
                    ProductCategory = category?.Title ?? "Unknown",              // หรือ category?.ToString() ?? ...
                    VariantText = variant == null
                                        ? "-"
                                        : $"{variant.Color} / {variant.Size}",
                    Quantity = i.quantity,
                    UnitPrice = i.price
                };
            })
            .ToList()
            };

            vm.ItemsTotal = vm.Items.Sum(i => i.LineTotal);
            ViewBag.SubTotal = order.totalAmount;
            ViewBag.Total = order.totalAmount + vm.ShippingFee;

            ViewBag.OrderStatusList = Enum.GetValues(typeof(OrderStatus))
        .Cast<OrderStatus>()
        .Select(s => new SelectListItem
        {
            Text = s.ToString(),
            Value = s.ToString(),
            Selected = s == order.status
        })
        .ToList();
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus newStatus)
        {
            var order = await _db.orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.status = newStatus;


            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(OrderDetail), new { id = orderId });
        }



        public async Task<IActionResult> Customer(string? keyword, int page = 1, int pageSize = 5)
        {
            var query = _userManager.Users.AsNoTracking();

            var keywordTrim = keyword?.Trim();


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var search = keyword.Trim();
                query = query.Where(c => c.Email.Contains(keywordTrim)
                                      || c.Name.Contains(keywordTrim)
                                      || c.UserName.Contains(keywordTrim)
                                      || c.Id.Contains(keywordTrim)
                                      );
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(customers);
        }
        // ใน AdminController.cs

        public async Task<IActionResult> CustomerDetail(string id, int page = 1, int pageSize = 5)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 1. สร้าง Query หลัก
            var ordersQuery = _db.orders.Where(o => o.customerId == id);

            // ---------------------------------------------------------
            // ส่วนที่เพิ่ม: การคำนวณสถิติ (Statistics)
            // ---------------------------------------------------------

            // 1. นับจำนวน Order ทั้งหมด
            var totalOrders = await ordersQuery.CountAsync();

            // 2. ยอดรวมการใช้จ่าย (ใส่ (decimal?) เพื่อป้องกัน error กรณีไม่มีข้อมูล)
            var totalSpending = await ordersQuery.SumAsync(o => (decimal?)o.totalAmount) ?? 0;

            // 3. แก้ไขตามสั่ง: นับสถานะ Delivered
            var completedCount = await ordersQuery.CountAsync(o => o.status == OrderStatus.delivered);

            // 4. แก้ไขตามสั่ง: นับสถานะ Canceled (L ตัวเดียว ตามคำสั่ง)
            var cancelledCount = await ordersQuery.CountAsync(o => o.status == OrderStatus.cancelled);

            // ---------------------------------------------------------

            // Pagination
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var pagedOrders = await ordersQuery
                .OrderByDescending(o => o.createdAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new CustomerVm
            {
                User = user,
                Form = new CustomerFormDto
                {
                    Role = user.Role,
                    Status = user.Status,
                    Id = user.Id
                },
                Orders = pagedOrders
            };

            // Dropdowns
            ViewBag.RoleList = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new SelectListItem
                {
                    Text = r.ToString(),
                    Value = r.ToString(),
                    Selected = (r == user.Role)
                }).ToList();

            ViewBag.StatusList = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = s.ToString(),
                    Selected = (s == user.Status)
                }).ToList();

            // ส่งค่า Pagination
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CustomerId = id;

            // ส่งค่าสถิติ
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalSpending = totalSpending;
            ViewBag.CompletedOrders = completedCount;
            ViewBag.CancelledOrders = cancelledCount;

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerDetail(CustomerVm vm)
        {
            var user = await _userManager.FindByIdAsync(vm.Form.Id);
            if (user == null) return NotFound();


            user.Role = vm.Form.Role;
            user.Status = vm.Form.Status;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                ModelState.AddModelError("", errors);

                ViewBag.RoleList = Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() })
                    .ToList();

                ViewBag.StatusList = Enum.GetValues(typeof(UserStatus))
                    .Cast<UserStatus>()
                    .Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() })
                    .ToList();

                vm.User ??= user;
                return View(vm);
            }

            // บันทึกสำเร็จ Redirect กลับมาหน้าเดิมเพื่อให้เห็นค่าใหม่
            return RedirectToAction("CustomerDetail" , "Admin" , new { id = user.Id });
        }


        [HttpGet]
        public async Task<IActionResult> Category()
        {

            var vm = new CategoryPageViewModel
            {
                Category = new Category(),
                Categories = await _db.Categories.ToListAsync()
            };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryPageViewModel vm)
        {
            var existingCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.Title == vm.Category.Title);

            if (existingCategory != null)
            {
                ModelState.AddModelError("Title", "A category with this title already exists.");
                return View("Category", vm);
            }

            if (!ModelState.IsValid)
            {
                return View("Category", vm);
            }



            await _db.Categories.AddAsync(vm.Category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Category));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {

            var result = await _db.Categories.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(result);
            _db.SaveChanges();
            return RedirectToAction(nameof(Category));

        }

    }


}
