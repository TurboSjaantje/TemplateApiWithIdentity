using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemplateApiWithIdentity.Authentication;

namespace TemplateApiWithIdentity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok("Anyone can access this.");
    }

    [HttpGet("user")]
    [Authorize(Policy = "User")]
    public IActionResult UserOnlyEndpoint()
    {
        return Ok("Only users with the 'User' role can access this.");
    }

    [HttpGet("admin")]
    [Authorize(Policy = "Admin")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("Only users with the 'Admin' role can access this.");
    }

    [HttpGet("any-authenticated")]
    [Authorize]
    public IActionResult AnyAuthenticatedUser()
    {
        return Ok($"Authenticated user: {User.Identity?.Name}");
    }
}