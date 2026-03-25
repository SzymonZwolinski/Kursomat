using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;

namespace Courses.Api.Endpoints;

public record GetCourseRequest(Guid Id);
public record GetCourseResponse(Guid Id, string Title, string Description, decimal Price);

[ApiController]
[AllowAnonymous]
public class GetCourseEndpoint : ControllerBase
{
    private readonly CoursesDbContext _dbContext;

    public GetCourseEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/api/courses/{Id}")]
    public async Task<IActionResult> HandleAsync([FromRoute] GetCourseRequest req, CancellationToken ct)
    {
        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (course is null)
        {
            return NotFound();
        }

        return Ok(new GetCourseResponse(course.Id, course.Title, course.Description, course.Price));
    }
}
