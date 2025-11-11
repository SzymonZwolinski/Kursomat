namespace Monolit.Features.Courses
{
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsPurchased { get; set; }
    }
}
