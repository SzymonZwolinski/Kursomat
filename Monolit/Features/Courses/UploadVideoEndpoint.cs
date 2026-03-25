using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monolit.DataBase;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Videos
{
    public class UploadVideoRequest
    {
        public Guid CourseId { get; set; }
        public IFormFile File { get; set; } = default!;
    }

    [ApiController]
    [Route("api/courses")]
    public class UploadVideoEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public UploadVideoEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpPost("{CourseId}/video")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, [FromForm] UploadVideoRequest req, CancellationToken ct)
        {
            var course = await _context.Courses.FindAsync(new object[] { CourseId }, ct);
            if (course == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Path.GetTempPath(), req.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await req.File.CopyToAsync(stream, ct);
            }

            return Ok();
        }
    }
}