using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace PetProj.Utils.Authentication;

public static class Authentication
{
	public static void SetAuthentication(IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthorization();
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:JwtToken").Value!));

			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = AuthOptions.ISSUER,
				ValidateAudience = true,
				ValidAudience = AuthOptions.AUDIENCE,
				ValidateLifetime = true,
				IssuerSigningKey = key,
				ValidateIssuerSigningKey = true
			};
		});
	}
}
