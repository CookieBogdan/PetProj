using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using PetProj.DLL.DbProviders;
using PetProj.Models;
using PetProj.Models.Account;
using PetProj.Models.Responses;
using PetProj.Utils;

using System.Text;

namespace PetProj.Controllers;

[Route("api/auth/yandex")]
[ApiController]
public sealed class OAuthYandexController : OAuthControllerBase
{
	private readonly IConfiguration _configuration;

	public OAuthYandexController(IJwtService jwtService, IAccountDbProvider accountDbProvider, IConfiguration configuration): base(accountDbProvider, jwtService)
	{
		_configuration = configuration;
	}

	protected override string ClientId => _configuration["YandexOAuth:ClientId"]!;
	protected override string ClientSecret => _configuration["YandexOAuth:ClientSecret"]!;


	[HttpGet, Route("login")]
	public RedirectResult Login(string state)
	{
		return Redirect($"https://oauth.yandex.ru/authorize?response_type=code&client_id={ClientId}&state={state}");
	}

	[HttpGet, Route("callback")]
	public async Task<IActionResult> Callback(string code, string state)
	{
		using var httpClient = new HttpClient();

		var plainTextBytes = Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}");
		var authorization = Convert.ToBase64String(plainTextBytes);
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authorization}");

		var dict = new Dictionary<string, string>
		{
			{ "grant_type", "authorization_code" },
			{ "code", code }
		};

		var response = await httpClient.PostAsync($"https://oauth.yandex.ru/token", new FormUrlEncodedContent(dict));
		var responseString = await response.Content.ReadAsStringAsync();
		var responseObj = JsonConvert.DeserializeAnonymousType(responseString, new
		{
			Access_token = ""
		});

		if (responseObj is null || responseObj.Access_token is null)
		{
			return BadRequest("yandex token dotn work");
		}

		string yandexAccessToken = responseObj.Access_token;

		httpClient.DefaultRequestHeaders.Clear();
		httpClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {yandexAccessToken}");

		response = await httpClient.GetAsync("https://login.yandex.ru/info?format=json");
		responseString = await response.Content.ReadAsStringAsync();
		var responseObjEmail = JsonConvert.DeserializeAnonymousType(responseString, new
		{
			Default_email = ""
		});

		if (responseObjEmail is null || responseObjEmail.Default_email is null)
		{
			return BadRequest("yandex info dotn work");
		}

		string jwt = await GenerateJwtAndRegistration(responseObjEmail.Default_email, AccountLocation.Yandex);

		Response.Cookies.Append("jwt", jwt);

		return Redirect(state);
	}

	[HttpGet, Route("confirm")]
	public async Task<IActionResult> ConfirmLogin()
	{
		Response.Cookies.Delete("jwt");
		var jwt = Request.Headers["jwt"].ToString();
		if (jwt is null)
		{
			BadRequest("Cookie is null");
		}
		
		AuthenticatedResponse authResponse = await GenerateAccessAndRefreshToken(jwt!);

		return Ok(authResponse);
	}
}
