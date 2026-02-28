using System.Collections.Generic;
using EcoSwap.Models;

namespace EcoSwap.Models;

public class AdminDashboardViewModel
{
    public List<User> Users { get; set; }
    public List<Product> Products { get; set; }
}