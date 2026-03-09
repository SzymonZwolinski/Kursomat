namespace Monolit.Features.Courses
{
    public class CourseDto
    {
        public Guid Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public bool IsPurchased { get; set; } = default!;
    }
}
