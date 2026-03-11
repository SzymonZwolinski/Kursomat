using System;
using System.Collections.Generic;

namespace Monolit.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public DateTime OrderDate { get; set; } = default!;
        public decimal TotalPrice { get; set; } = default!;
        public OrderStatus Status { get; set; } = default!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Created,
        Completed,
        Failed
    }
}
