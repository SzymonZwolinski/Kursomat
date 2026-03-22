using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Users.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class DeleteUserEndpoint : ControllerBase
    {
        private readonly UsersDbContext _context;

        public DeleteUserEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        [HttpDelete("/api/users/{UserId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId, ct);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);

            return Ok();
        }
    }
}
