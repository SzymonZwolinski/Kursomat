using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Carts.Endpoints
{
    public class RemoveFromCartRequest
    {
        public Guid UserId { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
    }

    internal class RemoveFromCartEndpoint : Endpoint<RemoveFromCartRequest>
    {
        private readonly CartsDbContext _context;

        public RemoveFromCartEndpoint(CartsDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/carts/{UserId}/items/{CourseId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RemoveFromCartRequest req, CancellationToken ct)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

            if (cart == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            var item = cart.Items.FirstOrDefault(i => i.CourseId == req.CourseId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _context.SaveChangesAsync(ct);
            }

            await Send.OkAsync(cancellation: ct);
        }
    }
}
