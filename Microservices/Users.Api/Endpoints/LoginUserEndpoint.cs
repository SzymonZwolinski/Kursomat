using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Api.Data;
using Users.Api.Helpers.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;

namespace Users.Api.Endpoints;

public record LoginUserRequest(string Login, string Password);
public record LoginUserResponse(string Token);

[ApiController]
[AllowAnonymous]
public class LoginUserEndpoint : ControllerBase
{
    private readonly UsersDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public LoginUserEndpoint(UsersDbContext dbContext, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("/api/users/login")]
    public async Task<IActionResult> HandleAsync([FromBody] LoginUserRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == req.Login, ct);

        if (user == null || !_passwordHasher.Verify(req.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = _configuration.GetSection("Jwt:Secret").Value ?? "FallbackSecretKeyForJwtAuthentication12345!@#";
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new LoginUserResponse(tokenString));
    }
}
