using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.DataBase;
using Monolit.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Courses
{
    public class CreateCourseRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
    }

    [ApiController]
    [Route("api/courses")]
    public class CreateCourseEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public CreateCourseEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromBody] CreateCourseRequest req, CancellationToken ct)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync(ct);

            var response = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                IsPurchased = false
            };

            return Ok(response);
        }
    }
}