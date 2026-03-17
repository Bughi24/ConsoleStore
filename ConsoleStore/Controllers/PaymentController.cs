using ConsoleStore.Data;
using ConsoleStore.Models;
using ConsoleStore.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleStore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ShoppingCart _cart;
        private readonly ConsoleStoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentController(ShoppingCart cart, ConsoleStoreContext context, UserManager<IdentityUser> userManager)
        {
            _cart = cart;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CaptureOrder([FromBody] Order data)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _cart.GetCartItems();

            if (!cartItems.Any())
                return BadRequest(new { success = false });

            var order = new Order
            {
                UserId = userId,
                CustomerName = data.CustomerName,
                Address = data.Address,
                City = data.City,
                Phone = data.Phone,
                OrderDat = DateTime.Now, 
                Items = new List<OrderItem>()
            };

            foreach (var i in cartItems)
            {
                var productInDb = await _context.Products.FindAsync(i.ProductId);

                if (productInDb != null)
                {
          
                    productInDb.Stock -= i.Quantity;

                    if (productInDb.Stock < 0) productInDb.Stock = 0;
                }

                order.Items.Add(new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                });
            }

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            await _cart.ClearCart();

            return Ok(new { success = true, orderId = order.OrderId });
        }

    }
}
  