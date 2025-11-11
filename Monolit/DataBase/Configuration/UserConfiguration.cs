using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monolit.Entities;

namespace Monolit.DataBase.Configuration
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(u => u.Login)
				.HasMaxLength(50);

			builder.Property(u => u.PasswordHash)
				.HasMaxLength(128);

			builder.HasIndex(u => u.Login)
				.IsUnique();

			builder.HasIndex(u => u.Email)
				.IsUnique();

			builder.Property(u => u.Login)
				.IsRequired();
		}
	}
}
