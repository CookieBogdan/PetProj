using PetProj.Models;

using System.Security.Claims;

namespace PetProj.Utils;

public interface IJwtService
{
	public string GenerateAccessToken(IEnumerable<Claim> claims);
	public string GenerateRefreshToken();
	public UserClaims GetUserClaimsFromExpiredToken(string token);
	public IEnumerable<Claim> CreateClaims(UserClaims claims);
}
