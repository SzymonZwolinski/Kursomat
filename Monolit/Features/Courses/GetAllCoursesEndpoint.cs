using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.Courses
{
    [ApiController]
    [Route("api/courses")]
    public class GetAllCoursesEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public GetAllCoursesEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("UserId");
            Guid.TryParse(userIdStr, out var userId);

            var userCourses = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync(ct);

            var courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    IsPurchased = userCourses.Contains(c.Id)
                })
                .ToListAsync(ct);

            return Ok(courses);
        }
    }
}
