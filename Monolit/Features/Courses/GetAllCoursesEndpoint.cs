using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;

namespace Monolit.Features.Courses
{
    public class GetAllCoursesEndpoint : EndpointWithoutRequest<List<CourseDto>>
    {
        private readonly MonolitDbContext _context;

        public GetAllCoursesEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/courses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync(ct);

            await SendAsync(courses, cancellation: ct);
        }
    }
}
