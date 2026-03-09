using System.ComponentModel.DataAnnotations;

namespace ConsoleStore.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Phone { get; set; }
        public string UserId { get; set; }

        public DateTime OrderDat { get; set; } = DateTime.Now;
        public List<OrderItem> Items { get; set; }
    }
}
