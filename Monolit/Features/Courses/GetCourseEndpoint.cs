using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Courses
{
    public class GetCourseRequest
    {
        public Guid Id { get; set; }
    }

    [ApiController]
    [Route("api/courses")]
    public class GetCourseEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public GetCourseEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid Id, CancellationToken ct)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == Id, ct);

            if (course == null)
            {
                return NotFound();
            }

            var response = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price
            };

            return Ok(response);
        }
    }
}