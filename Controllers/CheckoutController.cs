using ConsoleStore.Data;
using ConsoleStore.Models;
using ConsoleStore.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ConsoleStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly ShoppingCart _cart;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(ConsoleStoreContext context, ShoppingCart cart, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _cart = cart;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cart.GetCartItems();

            if(!cartItems.Any()) 
                return RedirectToAction("Index", "Store");

            decimal total = cartItems.Sum(i => i.Product.Price * i.Quantity);
       
            ViewBag.totalEUR = total;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string customerName, string address, string city, string phone)
        {
            var cartItems = await _cart.GetCartItems();

            if(!cartItems.Any())
            {
                return RedirectToAction("Index", "Store");
            }

            var order = new Order
            {
                CustomerName = customerName,
                Address = address,
                City = city,
                Phone = phone,
                UserId = _userManager.GetUserId(User),
                Items = new List<OrderItem>()
            };

            foreach (var item in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cart.ClearCart();

            return RedirectToAction("Success", new {id = order.OrderId});
        }

        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

    }
}
