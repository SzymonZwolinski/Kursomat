using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;

namespace Carts.Api.Endpoints;

public record GetCartRequest(Guid UserId);
public record GetCartResponse(Guid UserId, List<CartItemDto> Items);
public record CartItemDto(Guid CourseId, decimal Price);

public class GetCartEndpoint : Endpoint<GetCartRequest, GetCartResponse>
{
    private readonly CartsDbContext _dbContext;

    public GetCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/carts/{UserId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

        if (cart is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var items = cart.Items.Select(i => new CartItemDto(i.CourseId, i.Price)).ToList();
        await SendAsync(new GetCartResponse(cart.UserId, items), cancellation: ct);
    }
}
