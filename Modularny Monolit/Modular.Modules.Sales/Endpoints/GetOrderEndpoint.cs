using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Sales.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Sales.Endpoints
{
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
        public async Task<IActionResult> HandleAsync([FromRoute] Guid OrderId, CancellationToken ct)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == OrderId, ct);

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
