using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using EcoSwap.Models;

namespace EcoSwap.Models;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; } // Optional for editing

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required]
    public UserRole Role { get; set; }

    public IFormFile? ProfilePictureFile { get; set; }

    public string? ExistingProfilePictureFileName { get; set; }
}