using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Vex_E_commerce.Models
{

    public enum ProductStatus
    {
        Active,
        Inactive,
    }

    public enum ProductColor
    {
        Red,
        Black,
        White,
    }

    public enum ProductSize
    {
        XS,
        S,
        M,
        L,
        XL,
        XXL
    }


    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? TotalStock { get; set; } = 0;

        [MaxLength(255)]
        public string? PictureUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } = 0;

        public ProductStatus Status { get; set; } = ProductStatus.Active;

        // FK → Category
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // 1 Product → Many Variants
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ProductVariant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } = 0;

        [Required, MaxLength(50)]
        public string ProductTitle { get; set; } = "";


        public int Sold { get; set; } = 0;
        public int Stock { get; set; } = 0;

        public string Sku { get; set; }

        // FK → Product
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;

        // Enum (จะถูกเก็บเป็น string ใน DB)
        [Required]
        public ProductColor Color { get; set; }

        [Required]
        public ProductSize Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductFormVm
    {
        public Guid? Id { get; set; }
        // ฟิลด์จากฟอร์ม
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }


        [Required]
        public Guid CategoryId { get; set; }

        [Range(0, 999999)]
        public decimal? BasePrice { get; set; }

        // [Required]
        // [Url(ErrorMessage = "รูปแบบ URL ไม่ถูกต้อง")] 
        public string PhotoUrl { get; set; } = string.Empty;

        public string? VariantJson { get; set; }


        public List<TemporaryVariantData> Variants { get; set; } = new();

        

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }
  
    // เพิ่มคลาสนี้ใน Vex_E_commerce.Models namespace
    public class TemporaryVariantData
    {
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; } // ใช้ decimal เพื่อรองรับ BasePrice
        public int Stock { get; set; }
        public string Sku { get; set; } = string.Empty;
    }

}
