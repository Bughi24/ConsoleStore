using ConsoleStore.Data;
using ConsoleStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsoleStore.Controllers
{
    [Authorize]
    public class FavoriteItemsController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoriteItemsController(ConsoleStoreContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: My Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var favourites = await _context.FavoriteItems
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ToListAsync();

            return View(favourites);
        }

        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);

            bool productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists)
                return NotFound("Product does not exist!");

           
            var existing = await _context.FavoriteItems
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existing == null)
            {
                var fav = new FavoriteItem
                {
                    ProductId = productId,
                    UserId = userId
                };

                _context.FavoriteItems.Add(fav);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Store");
        }


       
        public async Task<IActionResult> Remove(int id)
        {
            var userId = _userManager.GetUserId(User);

            var fav = await _context.FavoriteItems
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (fav != null)
            {
                _context.FavoriteItems.Remove(fav);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
