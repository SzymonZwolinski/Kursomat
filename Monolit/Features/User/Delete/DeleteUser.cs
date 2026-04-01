using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Repositories.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.User.Delete
{
    public record DeleteUserRequest(Guid Id);

    [ApiController]
    [Route("api/users")]
    public class DeleteUser : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public DeleteUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _userRepository.DeleteUserAsync(id, ct);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
