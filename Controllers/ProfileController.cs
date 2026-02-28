using Microsoft.AspNetCore.Mvc;
using EcoSwap.Data;
using EcoSwap.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BCrypt.Net;

namespace EcoSwap.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly EcoSwapContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProfileController(EcoSwapContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.User.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.User.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ExistingProfilePictureFileName = user.ProfilePictureFileName
            };

            return View(viewModel);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || id != int.Parse(userId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _context.User.FindAsync(id);
                    if (userToUpdate == null)
                    {
                        return NotFound();
                    }

                    userToUpdate.Name = model.Name;
                    userToUpdate.Email = model.Email;

                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    // Handle profile picture upload
                    if (model.ProfilePictureFile != null)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(userToUpdate.ProfilePictureFileName))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, "images", userToUpdate.ProfilePictureFileName);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePictureFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.ProfilePictureFile.CopyToAsync(fileStream);
                        }
                        userToUpdate.ProfilePictureFileName = fileName;
                    }
                    else if (model.ExistingProfilePictureFileName == null && !string.IsNullOrEmpty(userToUpdate.ProfilePictureFileName))
                    {
                        // User removed the picture
                        string oldImagePath = Path.Combine(wwwRootPath, "images", userToUpdate.ProfilePictureFileName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                        userToUpdate.ProfilePictureFileName = null;
                    }

                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();

                    TempData["Msg"] = "Profile updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}