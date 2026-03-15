using FastEndpoints;
using Users.Api.Data;
using Users.Api.Entities;
using Users.Api.Helpers.Interfaces;

namespace Users.Api.Endpoints;

public record RegisterUserRequest(string Email, string Password, string FullName);
public record RegisterUserResponse(Guid Id);

public class RegisterUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    private readonly UsersDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserEndpoint(UsersDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
    {
        var passwordHash = _passwordHasher.Hash(req.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            PasswordHash = passwordHash,
            FullName = req.FullName
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(new RegisterUserResponse(user.Id), 201, cancellation: ct);
    }
}
