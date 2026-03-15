using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;

namespace Courses.Api.Endpoints;

public record CourseDto(Guid Id, string Title, string Description, decimal Price);

public class GetAllCoursesEndpoint : EndpointWithoutRequest<List<CourseDto>>
{
    private readonly CoursesDbContext _dbContext;

    public GetAllCoursesEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/courses");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var courses = await _dbContext.Courses
            .Select(c => new CourseDto(c.Id, c.Title, c.Description, c.Price))
            .ToListAsync(ct);

        await HttpContext.Response.SendAsync(courses, cancellation: ct);
    }
}
