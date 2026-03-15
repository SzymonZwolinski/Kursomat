using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Users.Api.Data;

namespace Users.Api.Endpoints;

public record UserDto(Guid Id, string Email, string FullName);

public class GetAllUsersEndpoint : EndpointWithoutRequest<List<UserDto>>
{
    private readonly UsersDbContext _dbContext;

    public GetAllUsersEndpoint(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _dbContext.Users
            .Select(u => new UserDto(u.Id, u.Email, u.FullName))
            .ToListAsync(ct);

        await HttpContext.Response.SendAsync(users, cancellation: ct);
    }
}
