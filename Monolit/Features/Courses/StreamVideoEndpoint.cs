using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Monolit.DataBase;

namespace Monolit.Features.Videos
{
    public class StreamVideoRequest
    {
        public Guid CourseId { get; set; }
    }

    public class StreamVideoEndpoint : Endpoint<StreamVideoRequest, IResult>
    {
        private readonly MonolitDbContext _context;

        public StreamVideoEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/courses/{CourseId}/video");
            AllowAnonymous();
        }

        public override async Task HandleAsync(StreamVideoRequest req, CancellationToken ct)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{req.CourseId}.mp4");

            if (!File.Exists(filePath))
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var result = Results.File(fileStream, contentType: "video/mp4", enableRangeProcessing: true);

            await Send.ResultAsync(result);
        }
    }
}