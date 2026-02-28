using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations; // Added for [Required]
using Microsoft.AspNetCore.Http;

namespace EcoSwap.Models;

public class Product
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Category { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public int Price { get; set; }
    public int? OriginalPrice { get; set; }
    [Range(0.0, 5.0)]
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    [Required]
    public double ImpactKg { get; set; }
    public string? ImageFileName { get; set; } // Made nullable
    [Required]
    public string Tags { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; } // Made nullable
}