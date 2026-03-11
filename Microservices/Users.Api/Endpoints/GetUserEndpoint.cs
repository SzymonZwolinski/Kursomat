using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Users.Api.Data;

namespace Users.Api.Endpoints;

public record GetUserRequest(Guid Id);
public record GetUserResponse(Guid Id, string Email, string FullName);

public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    private readonly UsersDbContext _dbContext;

    public GetUserEndpoint(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(new GetUserResponse(user.Id, user.Email, user.FullName), cancellation: ct);
    }
}
