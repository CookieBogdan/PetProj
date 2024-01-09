using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PetProj.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HomeController : ControllerBase
{
	[Route("/hello")]
	[HttpGet]
	
	public IActionResult Hello()
	{
		return Ok("Hello, world!");
	}
}
