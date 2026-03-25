using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Courses.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class GetCourseEndpoint : ControllerBase
    {
        private readonly CoursesDbContext _context;

        public GetCourseEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/courses/{CourseId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, CancellationToken ct)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == CourseId, ct);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price
            });
        }
    }
}
