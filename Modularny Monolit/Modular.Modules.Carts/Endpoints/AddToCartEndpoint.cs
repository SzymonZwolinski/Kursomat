using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using Modular.Modules.Carts.Entities;
using Modular.Modules.Courses.Contracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Modular.Modules.Carts.Endpoints
{
    public class AddToCartRequest
    {
        public Guid CourseId { get; set; }
    }

    [ApiController]
    [Route("api/carts")]
    [Authorize]
    public class AddToCartEndpoint : ControllerBase
    {
        private readonly CartsDbContext _context;
        private readonly ICoursesApi _coursesApi;

        public AddToCartEndpoint(CartsDbContext context, ICoursesApi coursesApi)
        {
            _context = context;
            _coursesApi = coursesApi;
        }

        [HttpPost("{UserId}/items")]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, [FromBody] AddToCartRequest req, CancellationToken ct)
        {
            var tokenUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("nameid");
            if (string.IsNullOrEmpty(tokenUserIdStr) || !Guid.TryParse(tokenUserIdStr, out var tokenUserId) || tokenUserId != UserId)
            {
                return Unauthorized();
            }

            var course = await _coursesApi.GetCourseAsync(req.CourseId, ct);
            if (course == null) return NotFound();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == UserId, ct);

            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = UserId };
                _context.Carts.Add(cart);
            }

            if (!cart.Items.Any(i => i.CourseId == req.CourseId))
            {
                cart.Items.Add(new CartItem { Id = Guid.NewGuid(), CourseId = req.CourseId });
                await _context.SaveChangesAsync(ct);
            }

            return Ok(new { CartId = cart.Id });
        }
    }
}