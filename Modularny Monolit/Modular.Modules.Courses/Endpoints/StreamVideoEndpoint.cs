using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Courses.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class StreamVideoEndpoint : ControllerBase
    {
        [HttpGet("/api/courses/{CourseId}/video")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, CancellationToken ct)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{CourseId}.mp4");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, "video/mp4", enableRangeProcessing: true);
        }
    }
}
