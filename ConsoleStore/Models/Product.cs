using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleStore.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [StringLength(1000)]
        public string? Specification { get; set; }

        [Range(0, int.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock {  get; set; }

        [StringLength(150)]
        public string? Supplier { get; set; }

        [StringLength(150)]
        public string? DeliveryMethod {get; set; }

        public string? PdfPath { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public string? VideoPath { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        
    }
}
