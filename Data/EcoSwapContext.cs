using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EcoSwap.Models;

namespace EcoSwap.Data
{
    public class EcoSwapContext : DbContext
    {
        public EcoSwapContext (DbContextOptions<EcoSwapContext> options)
            : base(options)
        {
        }

        public DbSet<EcoSwap.Models.Product> Product { get; set; } = default!;
        public DbSet<EcoSwap.Models.User> User { get; set; } = default!;
        public DbSet<EcoSwap.Models.CartItem> CartItem { get; set; } = default!;
    }
}
