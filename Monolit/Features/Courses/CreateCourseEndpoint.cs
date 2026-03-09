using FastEndpoints;
using Monolit.DataBase;
using Monolit.Entities;

namespace Monolit.Features.Courses
{
    public class CreateCourseRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
    }

    public class CreateCourseEndpoint : Endpoint<CreateCourseRequest, CourseDto>
    {
        private readonly MonolitDbContext _context;

        public CreateCourseEndpoint(MonolitDbContext context)
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
                Price = course.Price,
                IsPurchased = false
            };

            await Send.OkAsync(response, cancellation: ct);
        }
    }
}