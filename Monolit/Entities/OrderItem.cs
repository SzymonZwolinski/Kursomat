namespace Monolit.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } = default!;
        public Guid OrderId { get; set; } = default!;
        public Order Order { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
        public Course Course { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }
}
