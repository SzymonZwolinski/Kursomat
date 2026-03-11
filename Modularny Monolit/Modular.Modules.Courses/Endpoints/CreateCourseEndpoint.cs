using FastEndpoints;
using Modular.Modules.Courses.Data;
using Modular.Modules.Courses.Entities;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Courses.Endpoints
{
    public class CreateCourseRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    public class CourseDto
    {
        public Guid Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    internal class CreateCourseEndpoint : Endpoint<CreateCourseRequest, CourseDto>
    {
        private readonly CoursesDbContext _context;

        public CreateCourseEndpoint(CoursesDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/courses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateCourseRequest req, CancellationToken ct)
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync(ct);

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
