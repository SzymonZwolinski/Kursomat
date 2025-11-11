using System.ComponentModel.DataAnnotations;

namespace Monolit.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Login { get; set; }
		public string PasswordHash { get; set; }
		public string Email { get; set; }
		
		public User(string login, string passwordHash, string email)
		{
			Login = login;
			PasswordHash = passwordHash;
			Email = email;
		}
	}
}
