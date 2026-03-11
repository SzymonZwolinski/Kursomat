using System;

namespace Modular.Modules.Carts.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; } = default!;
        public Guid CartId { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
    }
}
