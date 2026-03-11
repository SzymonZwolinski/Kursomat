using System.Collections.Generic;

namespace Monolit.Entities
{
    public class Cart
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
