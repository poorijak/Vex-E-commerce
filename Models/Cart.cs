using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vex_E_commerce.Models
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ผู้ใช้ที่เป็นเจ้าของตะกร้า
        public string UserId { get; set; } = default!;
        public Customer User { get; set; } = default!;

        // รายการสินค้าในตะกร้า
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // ยอดรวมทั้งหมดของสินค้าในตะกร้า
        [Column(TypeName = "decimal(18,2)")]
        public decimal CartTotal { get; set; } = 1;


        // วันที่สร้าง
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // วันที่อัปเดตล่าสุด
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CartItem
    {
        public Guid Id { get; set; }  // EF จะสร้าง GUID เองเมื่อ Add()                  

        public int Count { get; set; }
        public string ProductTitle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = default!;

        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = default!;

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }


    public class CheckoutItemVm
    {
        public Guid CartItemId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string VariantText { get; set; } = string.Empty; // เช่น "Red / L"
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    // ViewModel Address สำหรับฟอร์มเท่านั้น (ไม่เกี่ยวกับ Entity)
    public class CheckoutAddressVm
    {
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string AddressDetail { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Province { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Note { get; set; }
    }

    public class CheckoutVm
    {
        public Guid CartId { get; set; }

        // ใช้ ViewModel แทน Address entity
        public CheckoutAddressVm Address { get; set; } = new();

        public List<CheckoutItemVm> Items { get; set; } = new();

        public decimal ItemsTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        public IFormFile? PaymentFile { get; set; }
    }

    public class myOrderVm
    {
        public Guid OrderId { get; set; }

        public List<OrderItem> Items { get; set; } = new();


    }


}


