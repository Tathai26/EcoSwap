using Microsoft.AspNetCore.Mvc;
using EcoSwap.Data;
using EcoSwap.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EcoSwap.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly EcoSwapContext _context;
    private readonly IWebHostEnvironment _hostEnvironment; // Injected IWebHostEnvironment

    public AdminController(EcoSwapContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment; // Assigned IWebHostEnvironment
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.User.ToListAsync();
        var products = await _context.Product.ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            Users = users,
            Products = products
        };

        return View(viewModel);
    }

    

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user != null)
        {
            if (user.Role == UserRole.User) // Only allow deletion of users with 'User' role
            {
                // Delete profile picture file when user is deleted
                if (!string.IsNullOrEmpty(user.ProfilePictureFileName))
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string imagePath = Path.Combine(wwwRootPath, "images", user.ProfilePictureFileName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.User.Remove(user);
            }
            else
            {
                TempData["Msg"] = "Only users with 'User' role can be deleted.";
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id)
    {
        return _context.User.Any(e => e.Id == id);
    }
}