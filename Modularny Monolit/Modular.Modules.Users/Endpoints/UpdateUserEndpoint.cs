using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Helpers.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Users.Endpoints
{
    public class UpdateUserRequest
    {
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Password { get; set; } = default!;
    }

    [ApiController]
    [AllowAnonymous]
    public class UpdateUserEndpoint : ControllerBase
    {
        private readonly UsersDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UpdateUserEndpoint(UsersDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPut("/api/users/{UserId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, [FromBody] UpdateUserRequest req, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId, ct);

            if (user == null)
            {
                return NotFound();
            }

            user.Login = req.Login;
            user.Email = req.Email;

            if (!string.IsNullOrEmpty(req.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(req.Password);
            }

            await _context.SaveChangesAsync(ct);
            return Ok();
        }
    }
}
