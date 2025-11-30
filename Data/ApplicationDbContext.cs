using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
    public DbSet<ProductWishlist> ProductWishlists { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<Address> addresses { get; set; }

    public DbSet<Order> orders { get; set; }

    public DbSet<OrderItem> orderItems { get; set; }

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

        builder.Entity<ProductWishlist>()
            .HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();

        builder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>().Property(p => p.Status).HasConversion<string>();
        builder.Entity<ProductVariant>().Property(v => v.Size).HasConversion<string>();
        builder.Entity<ProductVariant>().Property(v => v.Color).HasConversion<string>();

        builder.Entity<Customer>()
        .HasMany(c => c.addresses)
        .WithOne(a => a.Customer)
        .HasForeignKey(a => a.CustomerId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Order>()
          .HasOne(o => o.customer)
          .WithMany(c => c.Orders)
          .HasForeignKey(o => o.customerId)
          .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Order>()
            .HasOne(o => o.address)
            .WithOne(a => a.Order)
            .HasForeignKey<Order>(o => o.AddressId);

        builder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Order>().Property(o => o.status).HasConversion<string>();

    }


}
