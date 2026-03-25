using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;

namespace Courses.Api.Endpoints;

public record CourseDto(Guid Id, string Title, string Description, decimal Price);

[ApiController]
[AllowAnonymous]
public class GetAllCoursesEndpoint : ControllerBase
{
    private readonly CoursesDbContext _dbContext;

    public GetAllCoursesEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/api/courses")]
    public async Task<IActionResult> HandleAsync(CancellationToken ct)
    {
        var courses = await _dbContext.Courses
            .Select(c => new CourseDto(c.Id, c.Title, c.Description, c.Price))
            .ToListAsync(ct);

        return Ok(courses);
    }
}
