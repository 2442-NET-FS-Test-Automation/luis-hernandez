using Library.ControllerApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")] // localhost:5137/auth/edpoints/etc
public class AuthController : ControllerBase
{
    // Same constructuor injection as ny other controller. The token stuff is just another service behid an interface

    private readonly ITokenService _tokens;

    public AuthController(ITokenService tokens)
    {
        _tokens = tokens;
    }


    [HttpPost("token")]
    public ActionResult IssueToken(string userName)
    {
        // Get a new token
        var userToken = _tokens.Issue(userName);

        // Return it
        return Ok(userToken);
    }
}