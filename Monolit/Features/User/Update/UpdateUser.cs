using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Repositories.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.User.Update
{
    public record UpdateUserRequest(string Login, string Email, string? Password);

    [ApiController]
    [Route("api/users")]
    public class UpdateUser : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UpdateUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
        {
            var user = new Entities.User(req.Login, req.Password ?? string.Empty, req.Email)
            {
                Id = id
            };

            var result = await _userRepository.UpdateUserAsync(user, ct);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
