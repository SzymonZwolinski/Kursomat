using System;
using System.Collections.Generic;

namespace Modular.Modules.Carts.Entities
{
    public class Cart
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
