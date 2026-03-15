using FastEndpoints;
using Courses.Api.Data;
using Courses.Api.Entities;

namespace Courses.Api.Endpoints;

public record CreateCourseRequest(string Title, string Description, decimal Price);
public record CreateCourseResponse(Guid Id);

public class CreateCourseEndpoint : Endpoint<CreateCourseRequest, CreateCourseResponse>
{
    private readonly CoursesDbContext _dbContext;

    public CreateCourseEndpoint(CoursesDbContext dbContext)
    {
        _dbContext = dbContext;
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
            Title = req.Title,
            Description = req.Description,
            Price = req.Price
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(new CreateCourseResponse(course.Id), 201, cancellation: ct);
    }
}
