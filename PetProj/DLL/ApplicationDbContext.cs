using Microsoft.EntityFrameworkCore;

using PetProj.DLL.EFConfigurations;

namespace PetProj.DLL;

public class ApplicationDbContext : DbContext
{
	//DbSets

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration<AccountEFConfiguration>();
	}
}
