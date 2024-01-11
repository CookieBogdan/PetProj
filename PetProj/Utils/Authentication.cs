using Microsoft.AspNetCore.Authentication.JwtBearer;


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
