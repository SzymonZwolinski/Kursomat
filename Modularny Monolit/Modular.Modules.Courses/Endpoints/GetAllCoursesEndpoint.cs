using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Courses.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class GetAllCoursesEndpoint : ControllerBase
    {
        private readonly CoursesDbContext _context;

        public GetAllCoursesEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/courses")]
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price
                })
                .ToListAsync(ct);

            return Ok(courses);
        }
    }
}
