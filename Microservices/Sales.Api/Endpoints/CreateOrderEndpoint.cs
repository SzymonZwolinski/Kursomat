using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging.Events;
using Microservices.Sales.Api.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Sales.Api.Endpoints;

public record CreateOrderRequest(Guid? UserId, List<Guid> CourseIds, decimal TotalPrice);
public record CreateOrderResponse(Guid OrderId);

[ApiController]
[AllowAnonymous]
public class CreateOrderEndpoint : ControllerBase
{
    private readonly SalesDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderEndpoint(SalesDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("/api/orders")]
    public async Task<IActionResult> HandleAsync([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");

        if (string.IsNullOrEmpty(userIdStr) && Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString().Replace("Bearer ", "").Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(token))
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                userIdStr = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "UserId")?.Value;
            }
        }

        // 3. Fallback bezpieczeñstwa
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            userId = req.UserId ?? Guid.Empty;
        }

        var order = new Microservices.Sales.Api.Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalPrice = req.TotalPrice
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCompletedEvent(order.UserId, order.Id, req.CourseIds), ct);

        return Created($"/api/orders/{order.Id}", new CreateOrderResponse(order.Id));
    }
}