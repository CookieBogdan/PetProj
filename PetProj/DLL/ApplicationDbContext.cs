using Microsoft.EntityFrameworkCore;

using PetProj.DLL.EFConfigurations;
using PetProj.Models.Account;

namespace PetProj.DLL;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
	public DbSet<Account> Accounts { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new AccountEFConfiguration());
	}
}
