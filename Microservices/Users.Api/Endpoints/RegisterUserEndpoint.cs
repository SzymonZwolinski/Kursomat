using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Api.Data;
using Users.Api.Entities;
using Users.Api.Helpers.Interfaces;

namespace Users.Api.Endpoints;

public record RegisterUserRequest(string Email, string Password);
public record RegisterUserResponse(Guid Id);

[ApiController]
[AllowAnonymous]
public class RegisterUserEndpoint : ControllerBase
{
    private readonly UsersDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserEndpoint(UsersDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("/api/users/register")]
    public async Task<IActionResult> HandleAsync([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        var passwordHash = _passwordHasher.Hash(req.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            PasswordHash = passwordHash,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        return Created($"/api/users/{user.Id}", new RegisterUserResponse(user.Id));
    }
}