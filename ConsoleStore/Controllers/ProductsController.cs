using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConsoleStore.Data;
using ConsoleStore.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security;

namespace ConsoleStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ConsoleStoreContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ConsoleStoreContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var consoleStoreContext = _context.Products.Include(p => p.Category);
            return View(await consoleStoreContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile, IFormFile? pdfFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images/products");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    product.ImagePath = "/images/products/" + fileName;
                }

                if(pdfFile != null && pdfFile.Length > 0)
                {
                    string pdfFolder = Path.Combine(_env.WebRootPath, "pdf");

                    if(!Directory.Exists(pdfFolder))
                        Directory.CreateDirectory(pdfFolder);

                    string pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);

                    string pdfPath = Path.Combine(pdfFolder, pdfName);

                    using (var fileStream = new FileStream(pdfPath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    product.PdfPath = "/pdf/" + pdfName;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, IFormFile? pdfFile)
        {
            if (id != product.ProductId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null)
                        return NotFound();

                    // păstrăm valorile existente
                    product.ImagePath = existingProduct.ImagePath;
                    product.PdfPath = existingProduct.PdfPath;

                    // UPDATE IMAGE
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadFolder = Path.Combine(_env.WebRootPath, "images/products");

                        if (!Directory.Exists(uploadFolder))
                            Directory.CreateDirectory(uploadFolder);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        product.ImagePath = "/images/products/" + fileName;
                    }

                    // UPDATE PDF
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        string pdfFolder = Path.Combine(_env.WebRootPath, "pdf");

                        if (!Directory.Exists(pdfFolder))
                            Directory.CreateDirectory(pdfFolder);

                        string pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                        string pdfPath = Path.Combine(pdfFolder, pdfName);

                        using (var stream = new FileStream(pdfPath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(stream);
                        }

                        product.PdfPath = "/pdf/" + pdfName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
