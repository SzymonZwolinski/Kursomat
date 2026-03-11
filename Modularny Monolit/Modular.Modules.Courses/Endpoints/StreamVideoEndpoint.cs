using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Courses.Endpoints
{
    public class StreamVideoRequest
    {
        public Guid CourseId { get; set; } = default!;
    }

    internal class StreamVideoEndpoint : Endpoint<StreamVideoRequest>
    {
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
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await Send.StreamAsync(fileStream, fileName: $"{req.CourseId}.mp4", contentType: "video/mp4", enableRangeProcessing: true, cancellation: ct);
        }
    }
}
