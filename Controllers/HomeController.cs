using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoSwap.Models;
using System.Linq;
using System.Collections.Generic;
using EcoSwap.Data; // Added for EcoSwapContext
using Microsoft.EntityFrameworkCore; // Added for ToListAsync, FirstOrDefaultAsync

namespace EcoSwap.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EcoSwapContext _context; // Injected EcoSwapContext

    public HomeController(ILogger<HomeController> logger, EcoSwapContext context)
    {
        _logger = logger;
        _context = context; // Assigned EcoSwapContext
    }

    public async Task<IActionResult> Index()
    {
        var featuredProducts = await _context.Product.Where(p => p.Tags.Contains("bestseller")).ToListAsync();
        ViewBag.FeaturedProducts = featuredProducts;
        return View();
    }

    public async Task<IActionResult> Shop(string searchTerm, string category, string priceRange, string sortBy, string filter)
    {
        IQueryable<Product> products = _context.Product;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                                             p.Description.ToLower().Contains(searchTerm.ToLower()));
        }

        if (!string.IsNullOrEmpty(category))
        {
            products = products.Where(p => p.Category == category);
        }

        if (!string.IsNullOrEmpty(filter))
        {
            switch (filter)
            {
                case "bestseller":
                    products = products.Where(p => p.Tags.Contains("bestseller"));
                    break;
                case "new":
                    products = products.Where(p => p.Tags.Contains("new"));
                    break;
                case "local":
                    products = products.Where(p => p.Tags.Contains("local"));
                    break;
                case "high-impact":
                    products = products.Where(p => p.ImpactKg >= 5.0);
                    break;
                case "budget":
                    products = products.Where(p => p.Price <= 500);
                    break;
            }
        }

        if (!string.IsNullOrEmpty(priceRange))
        {
            switch (priceRange)
            {
                case "0-500":
                    products = products.Where(p => p.Price >= 0 && p.Price <= 500);
                    break;
                case "500-1000":
                    products = products.Where(p => p.Price > 500 && p.Price <= 1000);
                    break;
                case "1000-2000":
                    products = products.Where(p => p.Price > 1000 && p.Price <= 2000);
                    break;
                case "2000+":
                    products = products.Where(p => p.Price > 2000);
                    break;
            }
        }

        switch (sortBy)
        {
            case "price-low":
                products = products.OrderBy(p => p.Price);
                break;
            case "price-high":
                products = products.OrderByDescending(p => p.Price);
                break;
            case "rating":
                products = products.OrderByDescending(p => p.Rating);
                break;
            case "impact":
                products = products.OrderByDescending(p => p.ImpactKg);
                break;
            default:
                products = products.OrderByDescending(p => p.Tags.Contains("bestseller") ? 1 : 0);
                break;
        }

        ViewBag.CurrentSearch = searchTerm;
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentPriceRange = priceRange;
        ViewBag.CurrentSort = sortBy ?? "featured";
        ViewBag.CurrentFilter = filter;

        // Calculate total impact
        var totalImpact = await _context.Product.SumAsync(p => p.ImpactKg);
        ViewBag.TotalImpact = totalImpact;

        return View(await products.ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Product.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult About()
    {
        return View();
    }

}