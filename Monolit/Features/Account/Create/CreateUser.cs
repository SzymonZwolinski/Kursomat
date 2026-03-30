using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Monolit.Features.Account.Create
{
    public record CreateRequest(string Login, string Password, string Email);
	public record CreateResponse(Guid UserId);

	[ApiController]
	[Route("api/users")]
	public class CreateUser : ControllerBase
	{
		private readonly IUserRepository _accountRepository;
		private readonly IPasswordHasher _passwordHasher;

		public CreateUser(IUserRepository accountRepository, IPasswordHasher passwordHasher)
		{
			_accountRepository = accountRepository;
			_passwordHasher = passwordHasher;
		}

		[HttpPost("register")]
		[AllowAnonymous]
		public async Task<IActionResult> HandleAsync([FromBody] CreateRequest req, CancellationToken ct)
		{
			var passwordHash = _passwordHasher.HashPassword(req.Password);
			var user = new Entities.User(req.Login, passwordHash, req.Email);

			var userId = await _accountRepository.CreateAccountAsync(user, ct);
			var response = new CreateResponse(UserId: userId);

			return Ok(response);
		}
	}
}
