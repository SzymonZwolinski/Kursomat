using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [ApiController]
    [AllowAnonymous]
    public class GetOrderEndpoint : ControllerBase
    {
        private readonly SalesDbContext _context;

        public GetOrderEndpoint(SalesDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/orders/{OrderId}")]
        public async Task<IActionResult> HandleAsync([FromRoute] GetOrderRequest req, CancellationToken ct)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);

            if (order == null)
            {
                return NotFound();
            }

            var response = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice
            };

            return Ok(response);
        }
    }
}
