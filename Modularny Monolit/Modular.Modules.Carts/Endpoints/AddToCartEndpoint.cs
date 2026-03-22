using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using Modular.Modules.Carts.Entities;
using Modular.Modules.Courses.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Modular.Modules.Carts.Endpoints
{
    public class AddToCartRequest
    {
        public Guid UserId { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
    }

    [ApiController]
    [AllowAnonymous]
    public class AddToCartEndpoint : ControllerBase
    {
        private readonly CartsDbContext _context;
        private readonly ICoursesApi _coursesApi;

        public AddToCartEndpoint(CartsDbContext context, ICoursesApi coursesApi)
        {
            _context = context;
            _coursesApi = coursesApi;
        }

        [HttpPost("/api/carts/items")]
        public async Task<IActionResult> HandleAsync([FromBody] AddToCartRequest req, CancellationToken ct)
        {
            var course = await _coursesApi.GetCourseAsync(req.CourseId, ct);
            if (course == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = req.UserId };
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
