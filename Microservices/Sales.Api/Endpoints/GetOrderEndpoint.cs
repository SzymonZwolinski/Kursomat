using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microservices.Sales.Api.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microservices.Sales.Api.Endpoints
{
    public class GetOrderRequest
    {
        public Guid OrderId { get; set; } = default!;
    }

    public class OrderDto
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public decimal TotalPrice { get; set; } = default!;
    }

    public class GetOrderEndpoint : Endpoint<GetOrderRequest, OrderDto>
    {
        private readonly SalesDbContext _context;

        public GetOrderEndpoint(SalesDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/orders/{OrderId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrderRequest req, CancellationToken ct)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);

            if (order == null)
            {
                await HttpContext.Response.SendNotFoundAsync(cancellation: ct);
                return;
            }

            var response = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice
            };

            await HttpContext.Response.SendAsync(response, 200, cancellation: ct);
        }
    }
}
