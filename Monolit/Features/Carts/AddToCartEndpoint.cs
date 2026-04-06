using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Carts
{
    [ApiController]
    [Route("api/carts")]
    [Authorize]
    public class AddToCartEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public AddToCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpPost("{UserId}/items")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, [FromBody] AddToCartRequest req, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("nameid");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            if (UserId != userId)
            {
                return Forbid();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var course = await _context.Courses.FindAsync(new object[] { req.CourseId }, ct);
            if (course == null)
            {
                return NotFound();
            }

            if (!cart.Items.Any(i => i.CourseId == req.CourseId))
            {
                cart.Items.Add(new CartItem { CourseId = req.CourseId });
                await _context.SaveChangesAsync(ct);
            }

            return Ok();
        }
    }

    public class AddToCartRequest
    {
        public Guid CourseId { get; set; }
    }
}
