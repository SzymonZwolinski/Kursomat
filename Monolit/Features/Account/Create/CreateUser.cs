using FastEndpoints;
using Monolit.Entities;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.Account.Create
{
    public record CreateRequest(string Login, string Password, string Email);
	public record CreateResponse(Guid UserId);

	public class CreateUser : Endpoint<CreateRequest, CreateResponse>
	{
		private readonly IUserRepository _accountRepository;
		private readonly IPasswordHasher _passwordHasher;

		public CreateUser(IUserRepository accountRepository, IPasswordHasher passwordHasher)
		{
			_accountRepository = accountRepository;
			_passwordHasher = passwordHasher;
		}

		public override void Configure()
		{
			Post("/api/account/create");
			AllowAnonymous();
		}

		public override async Task HandleAsync(CreateRequest req, CancellationToken ct)
		{
			var passwordHash = _passwordHasher.HashPassword(req.Password);
			var user = new User(req.Login, passwordHash, req.Email);

			var userId = await _accountRepository.CreateAccountAsync(user, ct);
			var response = new CreateResponse(UserId: userId);

			await Send.OkAsync(response, ct);
		}
	}
}
