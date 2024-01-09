using PetProj.CLL;
using PetProj.DLL;
using PetProj.SLL;

namespace PetProj.Utils;
public static class ServiceRegistry
{
	public static void Register(IServiceCollection services)
	{
		//databases
		services.AddDbContext<ApplicationDbContext>();

		//dbProviders
		services.AddScoped<IUserDbProvider, FakeUserDbProvider>();
		services.AddScoped<IConfirmEmailRedisProvider, FakeConfirmEmailRedisProvider>();

		//senders
		services.AddScoped<IEmailSender, EmailSender>();
	}
}
