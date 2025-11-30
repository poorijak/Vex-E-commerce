using Microsoft.EntityFrameworkCore;
using Vex_E_commerce.Models;

namespace Vex_E_commerce.Data
{
    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                context.Database.Migrate();

                if (context.Categories.Any())
                {
                    return; // DB has been seeded
                }

                context.Categories.AddRange(new List<Category>()
                {
                    new Category { Title = "T-Shirts", CreatedAt = DateTime.UtcNow },
                    new Category { Title = "Sweater", CreatedAt = DateTime.UtcNow },
                    new Category { Title = "Hoodie", CreatedAt = DateTime.UtcNow },
                    new Category { Title = "Accessories", CreatedAt = DateTime.UtcNow }
                });

                context.SaveChanges();
            }
        }
    }
}