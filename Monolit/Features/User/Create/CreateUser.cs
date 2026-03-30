using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monolit.Features.User.Create
{
    public record CreateUserRequest(string Login, string Password, string Email);
	public record CreateUserResponse(Guid UserId);

	[ApiController]
	[Route("api/users")]
	public class CreateUser : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IPasswordHasher _passwordHasher;

		public CreateUser(IUserRepository userRepository, IPasswordHasher passwordHasher)
		{
			_userRepository = userRepository;
			_passwordHasher = passwordHasher;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> HandleAsync([FromBody] CreateUserRequest req, CancellationToken ct)
		{
			var passwordHash = _passwordHasher.HashPassword(req.Password);
			var user = new Entities.User(req.Login, passwordHash, req.Email);

			var userId = await _userRepository.CreateAccountAsync(user, ct);
			var response = new CreateUserResponse(UserId: userId);

			return Ok(response);
		}
	}
}
