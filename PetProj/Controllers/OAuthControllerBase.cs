using Microsoft.AspNetCore.Mvc;

using PetProj.DLL.DbProviders;
using PetProj.Models;
using PetProj.Models.Account;
using PetProj.Models.Responses;
using PetProj.Utils;

namespace PetProj.Controllers;

public abstract class OAuthControllerBase : ControllerBase
{
	private readonly IAccountDbProvider _accountDbProvider;
	private readonly IJwtService _jwtService;
	public OAuthControllerBase(IAccountDbProvider accountDbProvider, IJwtService jwtService)
	{
		_accountDbProvider = accountDbProvider;
		_jwtService = jwtService;
	}

	protected abstract string ClientId { get; }
	protected abstract string ClientSecret { get; }

	protected async Task<string> GenerateJwtAndRegistration(string accountEmail, AccountLocation providerLocation)
	{
		var account = await _accountDbProvider.GetAccountByEmailAsync(accountEmail);
		int accountId;
		if (account is not null)
		{
			if (account.AccountRegistrationLocation == providerLocation)
			{
				accountId = account.Id;
			}
			else
			{
				throw new Exception("Email exist.");
			}
		}
		else
		{
			accountId = await _accountDbProvider.CreateAccountAsync(new Account
			(
				accountEmail,
				"-",
				providerLocation
			));
		}

		var claims = _jwtService.CreateClaims(new UserClaims(accountId, accountEmail));
		return _jwtService.GenerateAccessToken(claims);
	}
	protected async Task<AuthenticatedResponse> GenerateAccessAndRefreshToken(string jwt)
	{
		var refreshToken = _jwtService.GenerateRefreshToken();
		var userCliams = _jwtService.GetUserClaimsFromExpiredToken(jwt!);
		await _accountDbProvider.UpdateAccountRefreshToken(userCliams.Id, refreshToken, _jwtService.RefreshTokenValidity);

		var accessToken = _jwtService.GenerateAccessToken(_jwtService.CreateClaims(userCliams));

		return (new AuthenticatedResponse(accessToken, refreshToken));
	}
}
