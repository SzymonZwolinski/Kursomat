using Microsoft.AspNetCore.Mvc;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Entities;
using Modular.Modules.Users.Helpers.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Users.Endpoints
{
    public class RegisterUserRequest
    {
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    [ApiController]
    [AllowAnonymous]
    public class RegisterUserEndpoint : ControllerBase
    {
        private readonly UsersDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserEndpoint(UsersDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("/api/users/register")]
        public async Task<IActionResult> HandleAsync([FromBody] RegisterUserRequest req, CancellationToken ct)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = req.Login,
                Email = req.Email,
                PasswordHash = _passwordHasher.HashPassword(req.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            return Ok();
        }
    }
}
