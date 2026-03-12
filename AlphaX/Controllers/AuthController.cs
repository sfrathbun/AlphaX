using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlphaX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            // Get the user from the ClaimsPrincipal set by the middleware
            var user = HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                id = userId,
                email = email ?? "",
                name = email?.Split('@')[0] ?? "User", // Extract name from email
                role = "user",
                permissions = new[] { "view_data" }
            });
        }
    }
}