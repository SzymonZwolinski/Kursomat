using Courses.Api.Data;
using Courses.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Courses.Api.Endpoints;

public record CreateCourseRequest(string Name, string Description, decimal Price, string? VideoPath);
public record CreateCourseResponse(Guid Id);

[ApiController]
[Authorize]
public class CreateCourseEndpoint : ControllerBase
{
    private readonly CoursesDbContext _dbContext;

    public CreateCourseEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("/api/courses")]
    public async Task<IActionResult> HandleAsync([FromBody] CreateCourseRequest req, CancellationToken ct)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = req.Name, 
            Description = req.Description,
            Price = req.Price,
            VideoPath = req.VideoPath ?? string.Empty
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(ct);

        return Created($"/api/courses/{course.Id}", new CreateCourseResponse(course.Id));
    }
}