using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcoSwap.Data;
using EcoSwap.Models;
using Microsoft.AspNetCore.Hosting; // Added for IWebHostEnvironment
using System.IO; // Added for Path and File operations

namespace EcoSwap.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EcoSwapContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Injected IWebHostEnvironment

        public ProductsController(EcoSwapContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment; // Assigned IWebHostEnvironment
        }

        // GET: Products
        public IActionResult Index()
        {
            return NotFound();
        }

        // GET: Products/Details/5
        public IActionResult Details(int? id)
        {
            return NotFound();
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return NotFound();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Category,Description,Price,OriginalPrice,Rating,ReviewCount,ImpactKg,ImageFileName,Tags,ImageFile")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Image upload logic
                if (product.ImageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImageFileName = fileName;
                }

                product.Tags = product.Tags ?? string.Empty; // Ensure Tags is not null

                _context.Add(product);
                await _context.SaveChangesAsync();
                return Json(new { success = true }); // Return JSON for AJAX success
            }

            // If ModelState is not valid, return JSON with errors
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            return Json(new { success = false, errors = errors });
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            return NotFound();
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,Description,Price,OriginalPrice,Rating,ReviewCount,ImpactKg,ImageFileName,Tags,ImageFile")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Image upload logic for Edit
                    if (product.ImageFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);

                        // Delete old image if it exists
                        var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageFileName))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, "images", existingProduct.ImageFileName);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fileStream);
                        }
                        product.ImageFileName = fileName;
                    }
                    else
                    {
                        // If no new image is uploaded, retain the existing ImageFileName
                        var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (existingProduct != null)
                        {
                            product.ImageFileName = existingProduct.ImageFileName;
                        }
                    }

                    product.Tags = product.Tags ?? string.Empty; // Ensure Tags is not null

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true }); // Return JSON for AJAX success
            }

            // If ModelState is not valid, return JSON with errors
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            return Json(new { success = false, errors = errors });
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int? id)
        {
            return NotFound();
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                // Delete image file when product is deleted
                if (!string.IsNullOrEmpty(product.ImageFileName))
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string imagePath = Path.Combine(wwwRootPath, "images", product.ImageFileName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
