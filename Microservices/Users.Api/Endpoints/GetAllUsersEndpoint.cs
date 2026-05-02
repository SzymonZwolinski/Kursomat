using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Api.Data;

namespace Users.Api.Endpoints;

public record UserDto(Guid Id, string Email);

[ApiController]
[AllowAnonymous]
public class GetAllUsersEndpoint : ControllerBase
{
    private readonly UsersDbContext _dbContext;

    public GetAllUsersEndpoint(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/api/users")]
    public async Task<IActionResult> HandleAsync(CancellationToken ct)
    {
        var users = await _dbContext.Users
            .Select(u => new UserDto(u.Id, u.Email))
            .ToListAsync(ct);

        return Ok(users);
    }
}
