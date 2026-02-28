using FastEndpoints;
using Modular.Modules.Courses.Contracts;
using Modular.Modules.Sales.Data;
using Modular.Modules.Sales.Entities;

namespace Modular.Modules.Sales.Endpoints
{
    public class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
    }

    internal class CreateOrderEndpoint : Endpoint<CreateOrderRequest>
    {
        private readonly SalesDbContext _context;
        private readonly ICoursesApi _coursesApi;

        public CreateOrderEndpoint(SalesDbContext context, ICoursesApi coursesApi)
        {
            _context = context;
            _coursesApi = coursesApi;
        }

        public override void Configure()
        {
            Post("/api/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
        {
            var course = await _coursesApi.GetCourseAsync(req.CourseId, ct);

            if (course == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                TotalPrice = course.Price
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(ct);

            await Send.OkAsync(new { OrderId = order.Id }, ct);
        }
    }
}