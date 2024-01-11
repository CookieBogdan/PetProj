using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetProj.Utils;

public class JwtService : IJwtService
{
	private const string ISSUER = "MyAuthServer"; // издатель токена
	private const string AUDIENCE = "MyAuthClient"; // потребитель токена

	private readonly IConfiguration _configuration;
	public JwtService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public string GenerateAccessToken(IEnumerable<Claim> claims)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:JwtSecretKey"]!));
		var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			claims: claims,
			issuer: ISSUER,
			audience: AUDIENCE,
			expires: DateTime.UtcNow.AddMinutes(15),
			signingCredentials: cred);

		string jwt = new JwtSecurityTokenHandler().WriteToken(token);
		return jwt;
	}

	//UNDONE: continue refresh token
	public string GenerateRefreshToken()
	{
		return Guid.NewGuid().ToString();
	}

	public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
	{
		var tokenValidationParameters = GetTokenValidationParameters(token);
		tokenValidationParameters.ValidateLifetime = false;

		var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

		var jwtSecurityToken = securityToken as JwtSecurityToken;
		if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
			throw new SecurityTokenException("Invalid token");
		return principal;
	}

	//TODO: i think static method in this class is not normal
	public static TokenValidationParameters GetTokenValidationParameters(string jwtSecretKey)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

		return new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = ISSUER,
			ValidateAudience = true,
			ValidAudience = AUDIENCE,
			ValidateLifetime = true,
			IssuerSigningKey = key,
			ValidateIssuerSigningKey = true
		};
	}
}
