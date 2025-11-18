namespace Vex_E_commerce.Models
{
    public class CategoryDetailViewModel
    {
        public Category Category { get; set; } = null!;
            public List<Product> Products { get; set; } = new();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    }
}
