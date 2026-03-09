using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modular.Modules.Courses.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Courses.Endpoints
{
    public class UploadVideoRequest
    {
        public Guid CourseId { get; set; } = default!;
        public IFormFile File { get; set; } = default!;
    }

    internal class UploadVideoEndpoint : Endpoint<UploadVideoRequest>
    {
        private readonly CoursesDbContext _context;

        public UploadVideoEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/courses/{CourseId}/video");
            AllowFileUploads();
            AllowAnonymous();
        }

        public override async Task HandleAsync(UploadVideoRequest req, CancellationToken ct)
        {
            var course = await _context.Courses.FindAsync(new object[] { req.CourseId }, ct);
            if (course == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            var filePath = Path.Combine(Path.GetTempPath(), $"{req.CourseId}.mp4");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await req.File.CopyToAsync(stream, ct);
            }

            await Send.OkAsync(ct);
        }
    }
}
