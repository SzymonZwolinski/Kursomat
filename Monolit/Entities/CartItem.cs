namespace Monolit.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; } = default!;
        public Guid CartId { get; set; } = default!;
        public Cart Cart { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
        public Course Course { get; set; } = default!;
    }
}
