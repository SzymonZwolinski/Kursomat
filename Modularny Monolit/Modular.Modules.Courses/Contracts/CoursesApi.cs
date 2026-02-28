using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Contracts;
using Modular.Modules.Courses.Data;

namespace Modular.Modules.Courses.Api
{
    internal class CoursesApi : ICoursesApi
    {
        private readonly CoursesDbContext _context;

        public CoursesApi(CoursesDbContext context)
        {
            _context = context;
        }

        public async Task<CourseDetailsDto?> GetCourseAsync(Guid courseId, CancellationToken ct = default)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId, ct);

            if (course == null) return null;

            return new CourseDetailsDto
            {
                Id = course.Id,
                Price = course.Price
            };
        }
    }
}