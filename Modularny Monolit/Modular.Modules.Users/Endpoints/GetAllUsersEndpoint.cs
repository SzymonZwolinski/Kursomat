using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Users.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class GetAllUsersEndpoint : ControllerBase
    {
        private readonly UsersDbContext _context;

        public GetAllUsersEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/users")]
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Login = u.Login,
                    Email = u.Email
                })
                .ToListAsync(ct);

            return Ok(users);
        }
    }
}
