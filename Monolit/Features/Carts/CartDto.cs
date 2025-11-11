using System.Collections.Generic;

namespace Monolit.Features.Carts
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
