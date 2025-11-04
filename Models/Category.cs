// Vex_E_commerce.Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace Vex_E_commerce.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MinLength(3, ErrorMessage = "Min 3 isus")]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> products { get; set; } = new List<Product>();
    }

    public class CategoryPageViewModel
    {
        public Category Category { get; set; } = new Category(); 

        public IEnumerable<Category> Categories { get; set; } = new List<Category>(); 
    }
}