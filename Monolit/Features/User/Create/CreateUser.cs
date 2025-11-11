using FastEndpoints;
using Monolit.Entities;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.User.Create
{
    public record CreateUserRequest(string Login, string Password, string Email);
	public record CreateUserResponse(Guid UserId);

	public class CreateUser : Endpoint<CreateUserRequest, CreateUserResponse>
	{
		private readonly IUserRepository _userRepository;
		private readonly IPasswordHasher _passwordHasher;

		public CreateUser(IUserRepository userRepository, IPasswordHasher passwordHasher)
		{
			_userRepository = userRepository;
			_passwordHasher = passwordHasher;
		}

		public override void Configure()
		{
			Post("/api/user");
			AllowAnonymous();
		}

		public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
		{
			var passwordHash = _passwordHasher.HashPassword(req.Password);
			var user = new User(req.Login, passwordHash, req.Email);

			var userId = await _userRepository.CreateAccountAsync(user, ct);
			var response = new CreateUserResponse(UserId: userId);

			await Send.OkAsync(response, ct);
		}
	}
}
