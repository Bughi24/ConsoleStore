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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

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
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile, IFormFile? pdfFile, IFormFile? videoFile,
            bool applyBlur = false, bool applyGrayscale = false, bool rotateImage = false)
        {
            if (ModelState.IsValid)
            {
            
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string fileName = Guid.NewGuid().ToString() + ".jpg";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = imageFile.OpenReadStream())
                    {
                        using (var image = await Image.LoadAsync(stream))
                        {
                            ApplyImageEffects(image, applyBlur, applyGrayscale, rotateImage);
                            await image.SaveAsJpegAsync(filePath);
                        }
                    }
                    product.ImagePath = "/images/products/" + fileName;
                }

                // Procesare PDF
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    string pdfFolder = Path.Combine(_env.WebRootPath, "pdf");
                    if (!Directory.Exists(pdfFolder)) Directory.CreateDirectory(pdfFolder);
                    string pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                    string pdfPath = Path.Combine(pdfFolder, pdfName);
                    using (var fileStream = new FileStream(pdfPath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }
                    product.PdfPath = "/pdf/" + pdfName;
                }

                // Procesare Video
                if (videoFile != null && videoFile.Length > 0)
                {
                    string videoFolder = Path.Combine(_env.WebRootPath, "videos");
                    if (!Directory.Exists(videoFolder)) Directory.CreateDirectory(videoFolder);

                    string videoName = Guid.NewGuid().ToString() + Path.GetExtension(videoFile.FileName);
                    string videoPath = Path.Combine(videoFolder, videoName);

                    using (var stream = new FileStream(videoPath, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }
                    product.VideoPath = "/videos/" + videoName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, IFormFile? pdfFile, IFormFile? videoFile,
            bool applyBlur = false, bool applyGrayscale = false, bool rotateImage = false)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
                    if (productToUpdate == null) return NotFound();

                    productToUpdate.Name = product.Name;
                    productToUpdate.Description = product.Description;
                    productToUpdate.Price = product.Price;
                    productToUpdate.CategoryId = product.CategoryId;

                    // Procesare Imagine
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadFolder = Path.Combine(_env.WebRootPath, "images/products");
                        string fileName = Guid.NewGuid().ToString() + ".jpg";
                        string filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = imageFile.OpenReadStream())
                        {
                            using (var image = await Image.LoadAsync(stream))
                            {
                                ApplyImageEffects(image, applyBlur, applyGrayscale, rotateImage);
                                await image.SaveAsJpegAsync(filePath);
                            }
                        }
                        productToUpdate.ImagePath = "/images/products/" + fileName;
                    }
                    else if (applyBlur || applyGrayscale || rotateImage)
                    {
                        if (!string.IsNullOrEmpty(productToUpdate.ImagePath))
                        {
                            string fullPath = Path.Combine(_env.WebRootPath, productToUpdate.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                string newFileName = Guid.NewGuid().ToString() + ".jpg";
                                string newPath = Path.Combine(_env.WebRootPath, "images/products", newFileName);

                                using (var image = await Image.LoadAsync(fullPath))
                                {
                                    ApplyImageEffects(image, applyBlur, applyGrayscale, rotateImage);
                                    await image.SaveAsJpegAsync(newPath);
                                }
                                productToUpdate.ImagePath = "/images/products/" + newFileName;
                            }
                        }
                    }

                    // Procesare PDF
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        string pdfFolder = Path.Combine(_env.WebRootPath, "pdf");
                        if (!Directory.Exists(pdfFolder)) Directory.CreateDirectory(pdfFolder);
                        string pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                        string pdfPath = Path.Combine(pdfFolder, pdfName);
                        using (var stream = new FileStream(pdfPath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(stream);
                        }
                        productToUpdate.PdfPath = "/pdf/" + pdfName;
                    }

                    // Procesare Video 
                    if (videoFile != null && videoFile.Length > 0)
                    {
                        string videoFolder = Path.Combine(_env.WebRootPath, "videos");
                        if (!Directory.Exists(videoFolder)) Directory.CreateDirectory(videoFolder);

                        string videoName = Guid.NewGuid().ToString() + Path.GetExtension(videoFile.FileName);
                        string videoPath = Path.Combine(videoFolder, videoName);

                        using (var stream = new FileStream(videoPath, FileMode.Create))
                        {
                            await videoFile.CopyToAsync(stream);
                        }
                        productToUpdate.VideoPath = "/videos/" + videoName;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        private void ApplyImageEffects(Image image, bool blur, bool gray, bool rotate)
        {
            image.Mutate(ctx => {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(500, 500),
                    Mode = ResizeMode.Max
                });

                if (blur) ctx.GaussianBlur(5f);
                if (gray) ctx.Grayscale();
                if (rotate) ctx.Rotate(RotateMode.Rotate90);
            });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}