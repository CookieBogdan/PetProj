using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

using PetProj.DLL;

namespace PetProj.Utils;

public static class Authentication
{
	public static void SetAuthentication(IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthorization();
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = JwtService.GetTokenValidationParameters(configuration["AppSettings:JwtSecretKey"]!);
			});
	}
}
