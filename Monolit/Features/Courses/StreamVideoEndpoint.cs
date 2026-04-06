using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monolit.DataBase;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Monolit.Features.Videos
{
    public class StreamVideoRequest
    {
        public Guid CourseId { get; set; }
    }

    [ApiController]
    [Route("api/courses")]
    public class StreamVideoEndpoint : ControllerBase
    {
        private readonly MonolitDbContext _context;

        public StreamVideoEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        [HttpGet("{CourseId}/video")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid CourseId, [FromQuery] string? access_token, CancellationToken ct)
        {
            var principal = User;
            if (principal?.Identity?.IsAuthenticated != true && !string.IsNullOrEmpty(access_token))
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                try
                {
                    var secret = "FallbackSecretKeyForJwtAuthentication12345!@#";
                    var tokenValidationParams = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                    principal = tokenHandler.ValidateToken(access_token, tokenValidationParams, out _);
                }
                catch
                {
                    return Unauthorized();
                }
            }

            if (principal?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("UserId");
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var hasPurchased = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == userId && uc.CourseId == CourseId, ct);

            if (!hasPurchased)
            {
                return Forbid();
            }

            var filePath = Path.Combine(Path.GetTempPath(), $"{CourseId}.mp4");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, contentType: "video/mp4", enableRangeProcessing: true);
        }
    }
}