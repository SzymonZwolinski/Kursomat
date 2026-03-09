using System;

namespace Modular.Modules.Sales.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public decimal TotalPrice { get; set; } = default!;
    }
}
