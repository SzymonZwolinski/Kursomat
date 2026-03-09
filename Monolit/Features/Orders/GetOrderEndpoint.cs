using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;

namespace Monolit.Features.Orders
{
    public class GetOrderRequest
    {
        public Guid Id { get; set; }
    }

    public class OrderItemDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = default!;
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class GetOrderEndpoint : Endpoint<GetOrderRequest, OrderDto>
    {
        private readonly MonolitDbContext _context;

        public GetOrderEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/orders/{Id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrderRequest req, CancellationToken ct)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == req.Id, ct);

            if (order == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var response = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    CourseId = oi.CourseId,
                    CourseName = oi.Course.Name
                }).ToList()
            };

            await Send.OkAsync(response, cancellation: ct);
        }
    }
}