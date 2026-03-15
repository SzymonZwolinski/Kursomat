using FastEndpoints;
using MassTransit;
using Shared.Messaging.Events;
using Microservices.Sales.Api.Data;
using Microservices.Sales.Api.Entities;

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
        var order = new Microservices.Sales.Api.Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = req.UserId,
            TotalPrice = req.TotalPrice
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCompletedEvent(order.UserId, order.Id, req.CourseIds), ct);

        await HttpContext.Response.SendAsync(new CreateOrderResponse(order.Id), 201, cancellation: ct);
    }
}
