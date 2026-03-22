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
    public class GetCartEndpoint : ControllerBase
    {
        private readonly CartsDbContext _context;

        public GetCartEndpoint(CartsDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/carts/{UserId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, CancellationToken ct)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == UserId, ct);

            if (cart == null)
            {
                return NotFound();
            }

            var response = new
            {
                CartId = cart.Id,
                Items = cart.Items.Select(i => new { i.Id, i.CourseId }).ToList()
            };

            return Ok(response);
        }
    }
}
