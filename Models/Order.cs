using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vex_E_commerce.Models
{
    public enum OrderStatus
    {
        pending,
        paid,
        shipped,
        delivered,
        cancelled
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal totalAmount { get; set; }

        public OrderStatus status { get; set; } = OrderStatus.pending;

        public string? paymentImage { get; set; }

        public DateTime? paymentAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        public decimal shippingFee { get; set; }

        public string? trackingNumver { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; }

        public string customerId { get; set; }
        public Customer customer { get; set; }


        public Guid AddressId { get; set; }
        public Address address { get; set; } = null;

    }

    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal price { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        public decimal totalPrice { get; set; }


        public Guid OrderId { get; set; }

        public Order Order { get; set; }

        public ProductVariant variant { get; set; }
        public Guid VariantId { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

    }


    public class OrderItemVm
    {
        public Guid OrderItemId { get; set; }

        public string PictureUrl { get; set; }

        public string ProductTitle { get; set; } = string.Empty;

        // เช่น "Red / L"
        public string VariantText { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }

    // ✅ เอาไว้โชว์ที่อยู่ (จะมาจาก Address ตัวจริง)
    public class OrderAddressVm
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    // ✅ ตัวหลักสำหรับหน้าแสดงผล / อัปโหลดสลิป
    public class OrderDetailVm
    {
        // ----- ข้อมูลออเดอร์หลัก -----
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentAt { get; set; }
        public string? TrackingNumber { get; set; }

        public int Tax { get; set; }

        // ----- ข้อมูลลูกค้า -----
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // ----- Address -----
        public OrderAddressVm Address { get; set; } = new();

        // ----- รายการสินค้า -----
        public List<OrderItemVm> Items { get; set; } = new();
        public decimal ItemsTotal { get; set; }

        // ----- รูปสลิป -----

        // ใช้ไว้รับไฟล์จากฟอร์ม
        public IFormFile? PaymentFile { get; set; }

        public string? PaymentImageUrl { get; set; }
    }

    public class ThankyouPageVm
    {
        public List<ProductCardVm> RelateProduct { get; set; } = new List<ProductCardVm>();
        public string OrderNumber { get; set; } = string.Empty;
    }

}