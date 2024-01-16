using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using PetProj.DLL.DbProviders;
using PetProj.Models.Account;
using PetProj.Models.Responses;
using PetProj.Utils;

using Swashbuckle.AspNetCore.Annotations;

namespace PetProj.Controllers;

[Route("api/auth/google")]
[ApiController]
public class OAuthGoogleController : OAuthControllerBase
{
	private readonly IConfiguration _configuration;

	public OAuthGoogleController(IJwtService jwtService, IAccountDbProvider accountDbProvider, IConfiguration configuration) : base(accountDbProvider, jwtService)
	{
		_configuration = configuration;
	}

	protected override string ClientId => _configuration["GoogleOAuth:ClientId"]!;
	protected override string ClientSecret => _configuration["GoogleOAuth:ClientSecret"]!;

	/// <summary>Redirect to google oauth2.</summary>
	[SwaggerResponse(StatusCodes.Status302Found)]
	[HttpGet, Route("login")]
	public RedirectResult Login(string state)
	{
		return Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&redirect_uri=https://localhost:7210/api/auth/google/callback&response_type=code&scope=https://www.googleapis.com/auth/userinfo.email&state={state}");
	}

	/// <summary>Callback method for google oauth2.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(AuthenticatedResponse))]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Problems with google.")]
	[SwaggerResponse(StatusCodes.Status302Found)]
	[HttpGet, Route("callback")]
	public async Task<IActionResult> Callback(string code, string state)
	{
		using var httpClient = new HttpClient();

		var dict = new Dictionary<string, string>
		{
			{ "client_id", ClientId },
			{ "client_secret", ClientSecret },
			{ "code", code },
			{ "grant_type", "authorization_code" },
			{ "redirect_uri", "https://localhost:7210/api/auth/google/callback" }
		};

		var response = await httpClient.PostAsync($"https://oauth2.googleapis.com/token", new FormUrlEncodedContent(dict));
		var responseString = await response.Content.ReadAsStringAsync();
		var responseObj = JsonConvert.DeserializeAnonymousType(responseString, new
		{
			Access_token = ""
		});

		if (responseObj is null || responseObj.Access_token is null)
		{
			return BadRequest("google token dotn work");
		}

		string googleAccessToken = responseObj.Access_token;

		httpClient.DefaultRequestHeaders.Clear();
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {googleAccessToken}");

		response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
		responseString = await response.Content.ReadAsStringAsync();
		var responseObjEmail = JsonConvert.DeserializeAnonymousType(responseString, new
		{
			Email = ""
		});

		if (responseObjEmail is null || responseObjEmail.Email is null)
		{
			return BadRequest("google userinfo dotn work");
		}

		string jwt = await GenerateJwtAndRegistration(responseObjEmail.Email, AccountLocation.Google);

		Response.Cookies.Append("jwt", jwt);

		return Redirect(state);
	}

	/// <summary>Confirm google oauth2 login, return JWTokens.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(AuthenticatedResponse))]
	[SwaggerResponse(StatusCodes.Status400BadRequest)]
	[HttpGet, Route("confirm")]
	public async Task<IActionResult> ConfirmLogin()
	{
		Response.Cookies.Delete("jwt");
		var jwt = Request.Headers["jwt"].ToString();
		if (jwt is null)
		{
			return BadRequest("Cookie is null");
		}

		AuthenticatedResponse authResponse = await GenerateAccessAndRefreshToken(jwt!);

		return Ok(authResponse);
	}
}
