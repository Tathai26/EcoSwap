using Microsoft.AspNetCore.Mvc;
using EcoSwap.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EcoSwap.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EcoSwap.Controllers
{
    public class CartController : Controller
    {
        private readonly EcoSwapContext _context;

        public CartController(EcoSwapContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            // This assumes you have authentication set up and UserId is available in Claims
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                // User is not logged in, redirect to login or show empty cart
                return View(new CartViewModel());
            }

            var cartItems = await _context.CartItem
                                          .Include(ci => ci.Product)
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();

            var cartViewModel = new CartViewModel
            {
                CartItems = cartItems
            };

            return View(cartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth"); // Redirect to login if not authenticated
            }

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound(); // Product not found
            }

            var cartItem = await _context.CartItem
                                         .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (cartItem == null)
            {
                // Add new item to cart
                cartItem = new Models.CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    DateCreated = System.DateTime.UtcNow
                };
                _context.CartItem.Add(cartItem);
            }
            else
            {
                // Update quantity of existing item
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // Redirect to cart page
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id) // 'id' here is CartItemId
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cartItem = await _context.CartItem
                                         .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

            if (cartItem == null)
            {
                return NotFound(); // Cart item not found or doesn't belong to user
            }

            _context.CartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity) // 'id' here is CartItemId
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cartItem = await _context.CartItem
                                         .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

            if (cartItem == null)
            {
                return NotFound(); // Cart item not found or doesn't belong to user
            }

            if (quantity <= 0)
            {
                _context.CartItem.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { itemCount = 0 });
            }

            var itemCount = await _context.CartItem
                                          .Where(ci => ci.UserId == userId)
                                          .SumAsync(ci => ci.Quantity);

            return Json(new { itemCount = itemCount });
        }
    }
}
