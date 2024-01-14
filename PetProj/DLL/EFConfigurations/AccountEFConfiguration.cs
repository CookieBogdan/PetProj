using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetProj.Models.Account;

namespace PetProj.DLL.EFConfigurations;

public class AccountEFConfiguration : IEntityTypeConfiguration<Account>
{
	public void Configure(EntityTypeBuilder<Account> builder)
	{
		builder.ToTable("Accounts");

		builder.HasKey(a => a.Id);
		builder.Property(a => a.Id).HasColumnName("AccountId");

		builder.Property(a => a.Email).IsRequired().HasMaxLength(100);
		builder.HasAlternateKey(a => a.Email);

		builder.Property(a => a.PasswordHash).IsRequired().HasMaxLength(100);

		builder.Property(a => a.AccountRegistrationLocation).IsRequired().HasDefaultValue(AccountLocation.None);

		builder.Property(a => a.RefreshToken).IsRequired(false).HasDefaultValue(null).HasMaxLength(100);

		builder.Property(a => a.RefreshTokenExpityTime).IsRequired(false).HasDefaultValue(null);
	}
}
