using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.DataBase;
using Monolit.Entities;
using Monolit.Features.Orders.Events;
using Monolit.Helpers.DomainEvents;
using System.Security.Claims;

namespace Monolit.Features.Orders
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class CreateOrderEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public CreateOrderEndpoint(MonolitDbContext context, IDomainEventDispatcher dispatcher)
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
                OrderDate = DateTime.UtcNow,
                TotalPrice = req.TotalPrice,
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>()
            };

            foreach (var courseId in req.CourseIds)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    CourseId = courseId,
                    Price = 99.99m
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            await _dispatcher.DispatchAsync(new OrderCompletedEvent(userId, order.Id, req.CourseIds), ct);

            return Ok(new CreateOrderResponse { OrderId = order.Id });
        }
    }

    public class CreateOrderRequest
    {
        public List<Guid> CourseIds { get; set; } = new List<Guid>();
        public decimal TotalPrice { get; set; }
    }

    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
    }
}