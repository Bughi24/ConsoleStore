using System.ComponentModel.DataAnnotations;

namespace ConsoleStore.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)] 
        public string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public ICollection<Product>? Products { get; set; }

    }
}
