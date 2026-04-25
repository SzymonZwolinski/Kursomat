using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("nameid");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null || !cart.Items.Any())
            {
                return NotFound();
            }

            var order = new Entities.Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.Items.Sum(i => i.Course.Price),
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>() 
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    CourseId = item.CourseId,
                    Price = item.Course.Price
                });
            }

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync(ct);

            var courseIds = order.Items.Select(i => i.CourseId).ToList();
            await _dispatcher.DispatchAsync(new OrderCompletedEvent(userId, order.Id, courseIds), ct);

            return Ok(new CreateOrderResponse { OrderId = order.Id });
        }
    }

    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
    }
}