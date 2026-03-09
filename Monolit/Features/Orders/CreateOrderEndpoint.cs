using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.Entities;
using Monolit.Features.Orders.Events;
using Monolit.Helpers.DomainEvents;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Orders
{
    public class CreateOrderEndpoint : EndpointWithoutRequest<CreateOrderResponse>
    {
        private readonly MonolitDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public CreateOrderEndpoint(MonolitDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        public override void Configure()
        {
            Post("/api/orders");
            Claims("UserId");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null || !cart.Items.Any())
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var order = new Entities.Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.Items.Sum(i => i.Course.Price),
                Status = OrderStatus.Completed // Simulated
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
            await _context.SaveChangesAsync(ct);

            // Dispatch domain event to handle side effects
            var courseIds = order.Items.Select(i => i.CourseId).ToList();
            await _dispatcher.DispatchAsync(new OrderCompletedEvent(userId, order.Id, courseIds), ct);

            await Send.OkAsync(new CreateOrderResponse { OrderId = order.Id }, cancellation: ct);
        }
    }

    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
    }
}
