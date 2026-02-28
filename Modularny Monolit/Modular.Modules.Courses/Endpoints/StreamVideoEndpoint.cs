using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Modular.Modules.Courses.Endpoints
{
    public class StreamVideoRequest
    {
        public Guid CourseId { get; set; }
    }

    internal class StreamVideoEndpoint : Endpoint<StreamVideoRequest, IResult>
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
                await SendNotFoundAsync(ct);
                return;
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var result = Results.File(fileStream, contentType: "video/mp4", enableRangeProcessing: true);

            await SendResultAsync(result);
        }
    }
}