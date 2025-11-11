using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System.Security.Claims;

namespace Monolit.Features.Carts
{
    public class RemoveFromCartEndpoint : Endpoint<RemoveFromCartRequest>
    {
        private readonly MonolitDbContext _context;

        public RemoveFromCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/cart/items/{CourseId}");
            Claims("UserId");
        }

        public override async Task HandleAsync(RemoveFromCartRequest req, CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue("UserId"));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var itemToRemove = cart.Items.FirstOrDefault(i => i.CourseId == req.CourseId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await _context.SaveChangesAsync(ct);
            }

            await SendOkAsync(ct);
        }
    }

    public class RemoveFromCartRequest
    {
        public Guid CourseId { get; set; }
    }
}
