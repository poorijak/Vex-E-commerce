using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Customer>().Property(u => u.Status).HasConversion<string>();
        builder.Entity<Customer>().Property(u => u.Role).HasConversion<string>();

        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // 👈 เปลี่ยนเป็น Cascade

        builder.Entity<Product>().Property(p => p.Status).HasConversion<string>();
        builder.Entity<ProductVariant>().Property(v => v.Size).HasConversion<string>();
        builder.Entity<ProductVariant>().Property(v => v.Color).HasConversion<string>();
    }


}
