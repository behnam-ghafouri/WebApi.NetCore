using CookieApi.Data;
using Microsoft.AspNetCore.Mvc;
namespace CookieApi.Controllers;
using System.Security.Claims;
using CookieApi.Data;
using CookieApi.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly CookieAuthContext _cookieAuthContext;
    public AuthController(CookieAuthContext cookieAuthContext)
    {
        _cookieAuthContext = cookieAuthContext;
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LoginDto login)
    {
		var test = "";
		var user = await _cookieAuthContext
			.User.Where(_ => _.Email.ToLower() == login.Email.ToLower() &&
			_.Password == login.Password && _.ExternalLoginName == null).FirstOrDefaultAsync();


		if (user == null)
		{
			return BadRequest("Invalid Credentials");
		}

		var claims = new List<Claim>
		{
			new Claim("userid", user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email)
		};

		var claimsIdentity = new ClaimsIdentity(
			claims, CookieAuthenticationDefaults.AuthenticationScheme);

		var authProperties = new AuthenticationProperties();

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			authProperties);
		return Ok("Success");

	}

	[HttpPost]
	[Route("logout")]
	public async Task<IActionResult> LogoutAsync()
	{
		await HttpContext.SignOutAsync();
		return Ok("success");
	}



	[Authorize]
	[HttpGet]
	[Route("user-profile")]
	public async Task<IActionResult> UserProfileAsync()
	{
		int userId = HttpContext.User.Claims
		.Where(_ => _.Type == "userid")
		.Select(_ => Convert.ToInt32(_.Value))
		.First();

		var userProfile = await _cookieAuthContext
		.User
		.Where(_ => _.Id == userId)
		.Select(_ => new UserProfileDto
		{
			UserId = _.Id,
			Email = _.Email,
			FirstName = _.FirstName,
			LastName = _.LastName
		}).FirstOrDefaultAsync();

		return Ok(userProfile);
	}


}