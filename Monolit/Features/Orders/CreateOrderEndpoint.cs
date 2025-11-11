using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.Entities;
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

        public CreateOrderEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/orders");
            Claims("UserId");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue("UserId"));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null || !cart.Items.Any())
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.Items.Sum(i => i.Course.Price),
                Status = OrderStatus.Created
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    CourseId = item.CourseId,
                    Price = item.Course.Price
                });
            }

            _context.Orders.Add(order);

            // Simulate payment processing
            order.Status = OrderStatus.Completed;

            foreach (var item in order.Items)
            {
                _context.UserCourses.Add(new UserCourse
                {
                    UserId = userId,
                    CourseId = item.CourseId
                });
            }

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync(ct);


            await SendAsync(new CreateOrderResponse { OrderId = order.Id }, cancellation: ct);
        }
    }

    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
    }
}
