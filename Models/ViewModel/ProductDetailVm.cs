using Vex_E_commerce.Models;

public class ProductDetailVm
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string PictureUrl { get; set; }
    public string Description { get; set; }
    public string CategoryTitle { get; set; }

    // เก็บ variant ของสินค้า
    public List<ProductVariantStockVm> Variants { get; set; } = new();
}

public class ProductVariantStockVm
{
    public Guid Id { get; set; }
    public ProductSize Size { get; set; }
    public ProductColor Color { get; set; }
    public int Stock { get; set; }
    public decimal BasePrice { get; set; }
}
