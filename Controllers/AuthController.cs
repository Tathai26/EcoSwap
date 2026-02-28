using Microsoft.AspNetCore.Mvc;
using EcoSwap.Data;
using EcoSwap.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BCrypt.Net; // For password hashing
using System.Security.Claims; // For authentication
using Microsoft.AspNetCore.Authentication; // For authentication
using Microsoft.AspNetCore.Authentication.Cookies; // For cookie authentication
using Microsoft.EntityFrameworkCore; // For database operations

namespace EcoSwap.Controllers
{
    public static class CustomClaimTypes
    {
        public const string ProfilePicture = "ProfilePicture";
    }

    public class AuthController : Controller
    {
        private readonly EcoSwapContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AuthController(EcoSwapContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                TempData["Msg"] = "Invalid credentials.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (!string.IsNullOrEmpty(user.ProfilePictureFileName))
            {
                claims.Add(new Claim(CustomClaimTypes.ProfilePicture, user.ProfilePictureFileName));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember me
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Cookie expiration
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["Msg"] = $"Logged in as {user.Name}.";
            return RedirectToAction("Index", "Home"); // Redirect to home page after successful login
        }

        [HttpGet]
        public IActionResult Signup() => View();

        [HttpPost]
        public async Task<IActionResult> Signup(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.User.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                string profilePictureFileName = null;
                if (model.ProfilePictureFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    profilePictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePictureFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", profilePictureFileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await model.ProfilePictureFile.CopyToAsync(fileStream);
                    }
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = UserRole.User,
                    ProfilePictureFileName = profilePictureFileName
                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Msg"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Msg"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["Msg"] = "Only admin can access admin panel.";
            return RedirectToAction("Index", "Home");
        }
    }
}
