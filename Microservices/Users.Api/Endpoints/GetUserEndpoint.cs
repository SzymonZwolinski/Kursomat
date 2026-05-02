using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Api.Data;

namespace Users.Api.Endpoints;

public record GetUserRequest(Guid Id);
public record GetUserResponse(Guid Id, string Email);

[ApiController]
[AllowAnonymous]
public class GetUserEndpoint : ControllerBase
{
    private readonly UsersDbContext _dbContext;

    public GetUserEndpoint(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/api/users/{Id}")]
    public async Task<IActionResult> HandleAsync([FromRoute] GetUserRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new GetUserResponse(user.Id, user.Email));
    }
}
