using ConsoleStore.Data;
using ConsoleStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleStore.Service
{
    public class ShoppingCart
    {
        private readonly ConsoleStoreContext _context;

        public ShoppingCart(ConsoleStoreContext context)
        {
            _context = context;
        }

        public string CartId { get; set; }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            var session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            var context = services.GetRequiredService<ConsoleStoreContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { CartId = cartId };
        }

        public async Task AddToCart(Product product)
        {
            var item = await _context.ShoppingCartItem.FirstOrDefaultAsync(c => c.ProductId == product.ProductId && c.CartId == CartId);

            if (item == null) {
                item = new ShoppingCartItem { ProductId = product.ProductId, CartId = CartId, Quantity = 1 };
                _context.ShoppingCartItem.Add(item);
            }
            else
            {
                item.Quantity++;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<ShoppingCartItem>> GetCartItems()
        {
            return await _context.ShoppingCartItem.Where(c => c.CartId == CartId)
                                                  .Include(c => c.Product)
                                                  .ToListAsync();
        }

        public async Task RemoveItem(int id)
        {
            var item = await _context.ShoppingCartItem.FirstOrDefaultAsync(i => i.Id == id);
            if (item != null)
            {
                _context.ShoppingCartItem.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCart()
        {
            var items = _context.ShoppingCartItem.Where(c => c.CartId == CartId);
            _context.ShoppingCartItem.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
