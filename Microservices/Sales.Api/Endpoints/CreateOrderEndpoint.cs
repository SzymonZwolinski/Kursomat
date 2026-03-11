using FastEndpoints;
using MassTransit;
using Shared.Messaging.Events;
using Sales.Api.Data;
using Sales.Api.Entities;

namespace Sales.Api.Endpoints;

public record CreateOrderRequest(Guid UserId, List<Guid> CourseIds, decimal TotalPrice);
public record CreateOrderResponse(Guid OrderId);

public class CreateOrderEndpoint : Endpoint<CreateOrderRequest, CreateOrderResponse>
{
    private readonly SalesDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderEndpoint(SalesDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public override void Configure()
    {
        Post("/api/orders");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = req.UserId,
            CourseIds = string.Join(",", req.CourseIds),
            TotalPrice = req.TotalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCompletedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            CourseIds = req.CourseIds
        }, ct);

        await SendAsync(new CreateOrderResponse(order.Id), 201, ct);
    }
}
