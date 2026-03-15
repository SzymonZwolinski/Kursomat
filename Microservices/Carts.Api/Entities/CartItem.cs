using System;

namespace Carts.Api.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid CourseId { get; set; }
    public decimal Price { get; set; }

    public Cart? Cart { get; set; }
}
