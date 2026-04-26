using Microsoft.AspNetCore.Mvc;
using Modular.Modules.Sales.Data;
using Modular.Shared.Events;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Modular.Modules.Sales.Endpoints
{
    public class CreateOrderRequest
    {
        public List<Guid> CourseIds { get; set; } = new List<Guid>();
        public decimal TotalPrice { get; set; }
    }

    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class CreateOrderEndpoint : ControllerBase
    {
        private readonly SalesDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public CreateOrderEndpoint(SalesDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> HandleAsync([FromBody] CreateOrderRequest req, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("nameid");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var order = new Entities.Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TotalPrice = req.TotalPrice
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            await _dispatcher.DispatchAsync(new OrderCompletedEvent(userId, order.Id, req.CourseIds), ct);

            return Ok(new { OrderId = order.Id });
        }
    }
}