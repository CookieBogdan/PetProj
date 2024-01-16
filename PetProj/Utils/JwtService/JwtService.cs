using Microsoft.IdentityModel.Tokens;

using PetProj.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetProj.Utils;

public class JwtService : IJwtService
{
	//TODO: issuer & audience - not normal values
	private const string ISSUER = "MyAuthServer"; // издатель токена
	private const string AUDIENCE = "MyAuthClient"; // потребитель токена
	private const string SecurityAlgorithm = SecurityAlgorithms.HmacSha256;

	private readonly IConfiguration _configuration;

	public DateTime RefreshTokenValidity => DateTime.UtcNow.AddMonths(1);
	private DateTime AccessTokenValidity => DateTime.UtcNow.AddMinutes(15);

	public JwtService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public string GenerateAccessToken(IEnumerable<Claim> claims)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:JwtSecretKey"]!));
		var cred = new SigningCredentials(key, SecurityAlgorithm);

		var token = new JwtSecurityToken(
			claims: claims,
			issuer: ISSUER,
			audience: AUDIENCE,
			expires: AccessTokenValidity,
			signingCredentials: cred);

		string jwt = new JwtSecurityTokenHandler().WriteToken(token);
		return jwt;
	}

	public string GenerateRefreshToken()
	{
		return Guid.NewGuid().ToString();
	}

	public UserClaims GetUserClaimsFromExpiredToken(string token)
	{
		var tokenValidationParameters = GetTokenValidationParameters(_configuration["AppSettings:JwtSecretKey"]!);
		tokenValidationParameters.ValidateLifetime = false;

		var claims = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken).Claims;
		//can invalid token -> exception

		if (securityToken is not JwtSecurityToken jwtSecurityToken ||
			!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new SecurityTokenException("Invalid token");
		}

		int accountId = int.Parse(claims.FirstOrDefault(c => c.Type == nameof(UserClaims.Id))?.Value ?? "0");
		string? email = claims.FirstOrDefault(c => c.Type == nameof(UserClaims.Email))?.Value;

		if (accountId == 0 || email is null)
			throw new ArgumentNullException("Claims not valid scheme.");

		return new UserClaims(accountId, email);
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
			ValidateIssuerSigningKey = true,
			ClockSkew = TimeSpan.Zero,
		};
	}

	public IEnumerable<Claim> CreateClaims(UserClaims claims)
	{
		return new List<Claim>
		{
			new(nameof(UserClaims.Id), claims.Id.ToString()),
			new(nameof(UserClaims.Email), claims.Email)
		};
	}
}
