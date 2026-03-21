using ConsoleStore.Data;
using ConsoleStore.Models;
using ConsoleStore.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace ConsoleStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly TfIdfService _tfidf;
        private readonly AutocompleteService _autocompleteService;
        private readonly RleCompressionService _rle;
        private readonly LuceneService _luceneService;
        public StoreController(ConsoleStoreContext context, TfIdfService tfIdf, AutocompleteService autocompleteService, RleCompressionService rle, LuceneService luceneService )
        {
            _context = context;
            _tfidf = tfIdf;
            _autocompleteService = autocompleteService;
            _rle = rle;
            _luceneService = luceneService;
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

        public async Task<IActionResult> AdvancedSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return RedirectToAction("Index");

           
            var luceneResults = _luceneService.SearchWithScore(query);

            if (luceneResults.Count == 0)
            {
                ViewBag.Message = "Nu s-au găsit rezultate în documentele PDF.";
                return View("AdvancedResults", new List<ProductScoreViewModel>());
            }

            var ids = luceneResults.Keys.ToList();

            
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();

            var model = products.Select(p => new ProductScoreViewModel
            {
                Product = p,
                Score = luceneResults.ContainsKey(p.ProductId) ? luceneResults[p.ProductId] : 0f
            })
            .OrderByDescending(x => x.Score) 
            .ToList();

            ViewBag.Query = query;
            return View("AdvancedResults", model);
        }

        [HttpGet]
        public async Task<IActionResult> Reindex()
        {
            var products = await _context.Products.ToListAsync();
            _luceneService.BuildIndex(products);
            return Content("Indexul Lucene a fost reconstruit cu succes!");
        }
    }
}
