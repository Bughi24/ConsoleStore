using ConsoleStore.Data;
using ConsoleStore.Models;
using ConsoleStore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsoleStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly ShoppingCart _cart;

        public CartController(ConsoleStoreContext context, ShoppingCart cart)
        {
            _context = context;
            _cart = cart;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var items = await _cart.GetCartItems();

            ViewBag.Total = items.Sum(i => i.Product.Price * i.Quantity);

            return View(items);
        }

        // ADD TO CART
        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            await _cart.AddToCart(product);

            return RedirectToAction("Index","Store");
        }

        // REMOVE one item
        public async Task<IActionResult> Remove(int id)
        {
            await _cart.RemoveItem(id);

            return RedirectToAction("Index");
        }

        // CLEAR CART
        public async Task<IActionResult> Clear()
        {
            await _cart.ClearCart();

            return RedirectToAction("Index");
        }
    }
}
