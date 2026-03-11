using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Courses.Endpoints
{
    public class GetCourseRequest
    {
        public Guid CourseId { get; set; } = default!;
    }

    internal class GetCourseEndpoint : Endpoint<GetCourseRequest, CourseDto>
    {
        private readonly CoursesDbContext _context;

        public GetCourseEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/courses/{CourseId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetCourseRequest req, CancellationToken ct)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.CourseId, ct);

            if (course == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            await Send.OkAsync(new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price
            }, cancellation: ct);
        }
    }
}
