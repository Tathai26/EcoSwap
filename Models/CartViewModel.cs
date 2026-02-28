using System.Collections.Generic;
using System.Linq;

namespace EcoSwap.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalPrice => CartItems.Sum(item => item.Product.Price * item.Quantity);
    }
}
