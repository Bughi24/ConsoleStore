using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConsoleStore.Data;
using Microsoft.Data.SqlClient;
using ConsoleStore.Service;


namespace ConsoleStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly TfIdfService _tfidf;
        private readonly AutocompleteService _autocompleteService;
        private readonly RleCompressionService _rle;
        public StoreController(ConsoleStoreContext context, TfIdfService tfIdf, AutocompleteService autocompleteService, RleCompressionService rle)
        {
            _context = context;
            _tfidf = tfIdf;
            _autocompleteService = autocompleteService;
            _rle = rle;
        }

        //GET: /Store
        public async Task<IActionResult> Index(int? categoryId,string? search, string? sortOrder)
        {
            var productsQuerry = _context.Products
                                         .Include(p => p.Category)
                                         .AsQueryable();

            if (categoryId != null)
            {
                productsQuerry = productsQuerry.Where(p => p.CategoryId == categoryId);
            }
            
            var productsList = await productsQuerry.ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsList = _tfidf.Search(search, productsList);
            }

            productsList = sortOrder switch
            {
                "price_asc" => productsList.OrderBy(p => p.Price).ToList(),
                "price_desc" => productsList.OrderByDescending(p => p.Price).ToList(),
                _             => productsList.OrderBy(p => p.ProductId).ToList()
            };
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;


            return View(productsList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);   

            if (product == null) 
                return NotFound();

            var allProducts = await _context.Products.ToListAsync();

            var similarProducts = _tfidf.GetSimilarProducts(product, allProducts);

            ViewBag.SimilarProducts = similarProducts;
            ViewBag.CompressedDescription = _rle.Compress(product.Description);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<string>());

            var products = await _context.Products.ToListAsync();

            var suggestions = _autocompleteService.GetSuggestions(term, products);

            return Json(suggestions);
        }
    }
}
