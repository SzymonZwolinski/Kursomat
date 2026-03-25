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
    public class GetCartEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public GetCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Course)
                .Where(c => c.UserId == userId)
                .Select(c => new CartDto
                {
                    Items = c.Items.Select(i => new CartItemDto
                    {
                        CourseId = i.CourseId,
                        CourseName = i.Course.Name,
                        CoursePrice = i.Course.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (cart == null)
            {
                cart = new CartDto();
            }

            return Ok(cart);
        }
    }
}
