using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Carts.Endpoints
{
    [ApiController]
    [AllowAnonymous]
    public class RemoveFromCartEndpoint : ControllerBase
    {
        private readonly CartsDbContext _context;

        public RemoveFromCartEndpoint(CartsDbContext context)
        {
            _context = context;
        }

        [HttpDelete("/api/carts/{UserId}/items/{CourseId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, [FromRoute] Guid CourseId, CancellationToken ct)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == UserId, ct);

            if (cart == null)
            {
                return NotFound();
            }

            var item = cart.Items.FirstOrDefault(i => i.CourseId == CourseId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _context.SaveChangesAsync(ct);
            }

            return Ok();
        }
    }
}
