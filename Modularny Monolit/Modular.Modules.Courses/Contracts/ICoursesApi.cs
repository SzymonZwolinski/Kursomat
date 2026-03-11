namespace Modular.Modules.Courses.Contracts
{
    public class CourseDetailsDto
    {
        public Guid Id { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    public interface ICoursesApi
    {
        Task<CourseDetailsDto?> GetCourseAsync(Guid courseId, CancellationToken ct = default);
    }
}