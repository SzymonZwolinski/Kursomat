using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Monolit.DataBase;

namespace Monolit.Features.Videos
{
    public class UploadVideoRequest
    {
        public Guid CourseId { get; set; }
        public IFormFile File { get; set; } = default!;
    }

    public class UploadVideoEndpoint : Endpoint<UploadVideoRequest>
    {
        private readonly MonolitDbContext _context;

        public UploadVideoEndpoint(MonolitDbContext context)
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
                await Send.NotFoundAsync(ct);
                return;
            }

            var filePath = Path.Combine(Path.GetTempPath(), req.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await req.File.CopyToAsync(stream, ct);
            }

            await Send.OkAsync(cancellation: ct);
        }
    }
}