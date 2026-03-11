using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Carts.Endpoints
{
    public class GetCartRequest
    {
        public Guid UserId { get; set; } = default!;
    }

    internal class GetCartEndpoint : Endpoint<GetCartRequest>
    {
        private readonly CartsDbContext _context;

        public GetCartEndpoint(CartsDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/carts/{UserId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetCartRequest req, CancellationToken ct)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

            if (cart == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            var response = new
            {
                CartId = cart.Id,
                Items = cart.Items.Select(i => new { i.Id, i.CourseId }).ToList()
            };

            await Send.OkAsync(response, cancellation: ct);
        }
    }
}
