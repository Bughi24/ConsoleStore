using ConsoleStore.Data;
using ConsoleStore.Models;
using ConsoleStore.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;

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
        [Authorize] 
        public async Task<IActionResult> Index(string customerName, string address, string city, string phone)
        {
            var cartItems = await _cart.GetCartItems();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Store");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        CustomerName = customerName,
                        Address = address,
                        City = city,
                        Phone = phone,
                        UserId = _userManager.GetUserId(User),
                        OrderDat = DateTime.Now,
                        Items = new List<OrderItem>()
                    };

                    foreach (var item in cartItems)
                    {
                  
                        var product = await _context.Products.FindAsync(item.ProductId);

                        if (product != null)
                        {
                            if (product.Stock < item.Quantity)
                            {
                                ModelState.AddModelError("", $"Stoc insuficient pentru produsul: {product.Name}");
                                return View(); 
                            }

                            product.Stock -= item.Quantity;
                        }

                        order.Items.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Product.Price
                        });
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    await _cart.ClearCart();

                    return RedirectToAction("Success", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "A apărut o eroare la procesarea comenzii. Vă rugăm să reîncercați.");
                    return View();
                }
            }
        }
        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

    }
}
