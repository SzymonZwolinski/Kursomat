using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;

namespace Courses.Api.Endpoints;

public record GetCourseRequest(Guid Id);
public record GetCourseResponse(Guid Id, string Title, string Description, decimal Price);

public class GetCourseEndpoint : Endpoint<GetCourseRequest, GetCourseResponse>
{
    private readonly CoursesDbContext _dbContext;

    public GetCourseEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/courses/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCourseRequest req, CancellationToken ct)
    {
        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (course is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(new GetCourseResponse(course.Id, course.Title, course.Description, course.Price), cancellation: ct);
    }
}
