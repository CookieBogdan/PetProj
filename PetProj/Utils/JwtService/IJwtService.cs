using System.Security.Claims;

namespace PetProj.Utils;

public interface IJwtService
{
	public string GenerateAccessToken(IEnumerable<Claim> claims);
	public string GenerateRefreshToken();
	public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
