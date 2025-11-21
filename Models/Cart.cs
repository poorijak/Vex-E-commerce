using System;
using System.Collections.Generic;
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
        public decimal UnitPrice { get; set; }
    }

}


