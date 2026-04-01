using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;

namespace Monolit.Features.Account.Login
{
    public record LoginRequest(string Login, string Password);
    public record LoginResponse(string Token);

    [ApiController]
    [Route("api/users")]
    public class LoginUser : ControllerBase
    {
        private readonly IUserRepository _accountRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public LoginUser(IUserRepository accountRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromBody] LoginRequest req, CancellationToken ct)
        {
            var user = await _accountRepository.GetUserByLoginAsync(req.Login, ct);

            if (user == null || !_passwordHasher.VerifyPassword(req.Password, user.PasswordHash))
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
                    new Claim(ClaimTypes.Name, user.Login)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var response = new LoginResponse(Token: tokenString);

            return Ok(response);
        }
    }
}
