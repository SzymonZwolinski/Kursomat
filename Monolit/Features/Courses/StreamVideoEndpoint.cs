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
    public class StreamVideoRequest
    {
        public Guid CourseId { get; set; }
    }

    [ApiController]
    [Route("api/courses")]
    public class StreamVideoEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public StreamVideoEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet("{CourseId}/video")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, CancellationToken ct)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{CourseId}.mp4");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, contentType: "video/mp4", enableRangeProcessing: true);
        }
    }
}