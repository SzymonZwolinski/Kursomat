using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Carts
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class RemoveFromCartEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public RemoveFromCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpDelete("items/{CourseId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null)
            {
                return NotFound();
            }

            var itemToRemove = cart.Items.FirstOrDefault(i => i.CourseId == CourseId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await _context.SaveChangesAsync(ct);
            }

            return Ok();
        }
    }

    public class RemoveFromCartRequest
    {
        public Guid CourseId { get; set; }
    }
}
