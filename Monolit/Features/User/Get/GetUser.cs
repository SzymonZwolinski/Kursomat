using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Repositories.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.User.Get
{
    public record GetUserRequest(Guid Id);
    public record GetUserResponse(Guid Id, string Login, string Email);

    [ApiController]
    [Route("api/users")]
    public class GetUser : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public GetUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync([FromRoute] Guid id, CancellationToken ct)
        {
            var user = await _userRepository.GetUserByIdAsync(id, ct);
            if (user is null)
            {
                return NotFound();
            }

            var response = new GetUserResponse(user.Id, user.Login, user.Email);
            return Ok(response);
        }
    }
}
