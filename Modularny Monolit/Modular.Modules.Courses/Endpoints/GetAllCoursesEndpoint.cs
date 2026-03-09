using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Courses.Endpoints
{
    internal class GetAllCoursesEndpoint : EndpointWithoutRequest<List<CourseDto>>
    {
        private readonly CoursesDbContext _context;

        public GetAllCoursesEndpoint(CoursesDbContext context)
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
                    Description = c.Description,
                    Price = c.Price
                })
                .ToListAsync(ct);

            await Send.OkAsync(courses, cancellation: ct);
        }
    }
}
