using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Users.Endpoints
{
    public class UserDto
    {
        public Guid Id { get; set; } = default!;
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    [ApiController]
    [AllowAnonymous]
    public class GetUserEndpoint : ControllerBase
    {
        private readonly UsersDbContext _context;

        public GetUserEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/users/{UserId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, CancellationToken ct)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == UserId, ct);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email
            });
        }
    }
}
