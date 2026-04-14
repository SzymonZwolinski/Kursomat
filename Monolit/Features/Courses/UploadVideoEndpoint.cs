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
        /*public Guid CourseId { get; set; }*/
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
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, [FromForm] IFormFile file, CancellationToken ct)
        {
            var course = await _context.Courses.FindAsync(new object[] { CourseId }, ct);

            if (course == null) return NotFound();
            if (file == null) return BadRequest();

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            course.VideoPath = filePath;
            await _context.SaveChangesAsync(ct);

            return Ok();
        }

        /* [HttpPost("{CourseId}/video")]
         [AllowAnonymous]
         [IgnoreAntiforgeryToken]
         public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, CancellationToken ct)
         {
             Console.WriteLine($"--- WEJŒCIE DO HANDLERA: {CourseId} ---");

             var test = Request;
             return Ok();
             //var file = Request.Form.Files.GetFile("file"); // Pobranie rêczne po nazwie klucza
 *//*
             if (file == null) return BadRequest("Nie znaleziono pliku w Request.Form.Files");

             var course = await _context.Courses.FindAsync(new object[] { CourseId }, ct);
             if (course == null) return NotFound();

             var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
             using (var stream = new FileStream(filePath, FileMode.Create))
             {
                 await file.CopyToAsync(stream, ct);
             }

             course.VideoPath = filePath;
             await _context.SaveChangesAsync(ct);

             return Ok();*//*
         }*/
    }
}