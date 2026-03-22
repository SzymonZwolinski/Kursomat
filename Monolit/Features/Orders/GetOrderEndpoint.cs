using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    [ApiController]
    [Route("api/orders")]
    public class GetOrderEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public GetOrderEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid Id, CancellationToken ct)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == Id, ct);

            if (order == null)
            {
                return NotFound();
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

            return Ok(response);
        }
    }
}