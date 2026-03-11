using FastEndpoints;
using Modular.Modules.Carts.Contracts;
using Modular.Modules.Courses.Contracts;
using Modular.Modules.Sales.Data;
using Modular.Modules.Sales.Entities;
using Modular.Shared.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Sales.Endpoints
{
    public class CreateOrderRequest
    {
        public Guid UserId { get; set; } = default!;
    }

    internal class CreateOrderEndpoint : Endpoint<CreateOrderRequest>
    {
        private readonly SalesDbContext _context;
        private readonly ICartsApi _cartsApi;
        private readonly ICoursesApi _coursesApi;
        private readonly IDomainEventDispatcher _dispatcher;

        public CreateOrderEndpoint(
            SalesDbContext context,
            ICartsApi cartsApi,
            ICoursesApi coursesApi,
            IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _cartsApi = cartsApi;
            _coursesApi = coursesApi;
            _dispatcher = dispatcher;
        }

        public override void Configure()
        {
            Post("/api/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
        {
            var cart = await _cartsApi.GetCartAsync(req.UserId, ct);

            if (cart == null || !cart.CourseIds.Any())
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            decimal totalPrice = 0;
            foreach (var courseId in cart.CourseIds)
            {
                var course = await _coursesApi.GetCourseAsync(courseId, ct);
                if (course != null)
                {
                    totalPrice += course.Price;
                }
            }

            var order = new Entities.Order
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                TotalPrice = totalPrice
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            await _dispatcher.DispatchAsync(new OrderCompletedEvent(req.UserId, order.Id, cart.CourseIds), ct);

            await Send.OkAsync(new { OrderId = order.Id }, cancellation: ct);
        }
    }
}
