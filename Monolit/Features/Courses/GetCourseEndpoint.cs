using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;

namespace Monolit.Features.Courses
{
    public class GetCourseRequest
    {
        public Guid Id { get; set; }
    }

    public class GetCourseEndpoint : Endpoint<GetCourseRequest, CourseDto>
    {
        private readonly MonolitDbContext _context;

        public GetCourseEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/courses/{Id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetCourseRequest req, CancellationToken ct)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

            if (course == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var response = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price
            };

            await Send.OkAsync(response, cancellation: ct);
        }
    }
}