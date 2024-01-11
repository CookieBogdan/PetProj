using Microsoft.EntityFrameworkCore;

using PetProj.CLL;
using PetProj.DLL;
using PetProj.DLL.DbProviders;
using PetProj.SLL;
using PetProj.Utils;

namespace PetProj.Utils;
public static class ServiceRegistry
{
	public static void Register(IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<IConfiguration>(sp => configuration);
		services.AddSingleton<IJwtService, JwtService>();

		//databases
		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseNpgsql(configuration["AppSettings:ConnectionString"]);
		});

		//dbProviders
		services.AddScoped<IAccountDbProvider, AccountDbProvider>();
		services.AddScoped<IConfirmEmailRedisProvider, FakeConfirmEmailRedisProvider>();

		//senders
		services.AddScoped<IEmailSender, EmailSender>();
	}
}
