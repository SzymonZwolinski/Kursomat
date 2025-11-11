using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System.Security.Claims;

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
            AllowAnonymous(); // Or use Claims("UserId") if you want to require authentication
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("UserId");
            Guid.TryParse(userIdStr, out var userId);

            var userCourses = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync(ct);

            var courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    IsPurchased = userCourses.Contains(c.Id)
                })
                .ToListAsync(ct);

            await Send.OkAsync(courses, cancellation: ct);
        }
    }
}
