namespace Modular.Modules.Courses.Contracts
{
    public class CourseDetailsDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
    }

    public interface ICoursesApi
    {
        Task<CourseDetailsDto?> GetCourseAsync(Guid courseId, CancellationToken ct = default);
    }
}