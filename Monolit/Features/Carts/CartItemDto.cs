namespace Monolit.Features.Carts
{
    public class CartItemDto
    {
        public Guid CourseId { get; set; } = default!;
        public string CourseName { get; set; } = default!;
        public decimal CoursePrice { get; set; } = default!;
    }
}
