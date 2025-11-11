using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.Entities;
using System.Security.Claims;

namespace Monolit.Features.Carts
{
    public class AddToCartEndpoint : Endpoint<AddToCartRequest>
    {
        private readonly MonolitDbContext _context;

        public AddToCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/cart/items");
            Claims("UserId");
        }

        public override async Task HandleAsync(AddToCartRequest req, CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue("UserId"));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var course = await _context.Courses.FindAsync(req.CourseId);
            if (course == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (!cart.Items.Any(i => i.CourseId == req.CourseId))
            {
                cart.Items.Add(new CartItem { CourseId = req.CourseId });
                await _context.SaveChangesAsync(ct);
            }

            await SendOkAsync(ct);
        }
    }

    public class AddToCartRequest
    {
        public Guid CourseId { get; set; }
    }
}
