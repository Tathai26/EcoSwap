using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EcoSwap.Models;

public enum UserRole
{
    Admin,
    User
}

public class User
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public UserRole Role { get; set; }

    public string? ProfilePictureFileName { get; set; }

    [NotMapped]
    public IFormFile? ProfilePictureFile { get; set; }
}