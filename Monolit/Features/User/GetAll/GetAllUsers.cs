using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Features.User.Get;
using Monolit.Helpers.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.User.GetAll
{
    public record GetAllUsersResponse(IEnumerable<GetUserResponse> Users);

    [ApiController]
    [Route("api/user")]
    public class GetAllUsers : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsers(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> HandleAsync(CancellationToken ct)
        {
            var users = await _userRepository.GetAllUsersAsync(ct);
            var response = new GetAllUsersResponse(users.Select(x => new GetUserResponse(x.Id, x.Login, x.Email)));
            return Ok(response);
        }
    }
}
