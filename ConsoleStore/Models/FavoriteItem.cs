using System.ComponentModel.DataAnnotations;

namespace ConsoleStore.Models
{
    public class FavoriteItem
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
