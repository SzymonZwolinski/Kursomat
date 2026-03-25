using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Modular.Modules.Courses.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Courses.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class UploadVideoEndpoint : ControllerBase
    {
        private readonly CoursesDbContext _context;

        public UploadVideoEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/courses/{CourseId}/video")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, [FromForm] IFormFile file, CancellationToken ct)
        {
            var course = await _context.Courses.FindAsync(new object[] { CourseId }, ct);
            if (course == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Path.GetTempPath(), $"{CourseId}.mp4");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            return Ok();
        }
    }
}
