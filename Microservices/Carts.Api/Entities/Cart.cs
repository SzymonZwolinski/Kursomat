using System;
using System.Collections.Generic;

namespace Carts.Api.Entities;

public class Cart
{
    public Guid UserId { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
