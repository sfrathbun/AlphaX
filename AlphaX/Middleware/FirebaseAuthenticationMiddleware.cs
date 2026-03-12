using FirebaseAdmin.Auth;
using System.Security.Claims;

namespace AlphaX.Middleware
{
    public class FirebaseAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAuthenticationMiddleware> _logger;

        public FirebaseAuthenticationMiddleware(RequestDelegate next, ILogger<FirebaseAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractToken(context.Request.Headers);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                    // Attach Firebase user info to context
                    context.Items["FirebaseUser"] = decodedToken;

                    // Optional: Create claims for authorization
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                        new Claim(ClaimTypes.Email, decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : "")
                    };
                    var identity = new ClaimsIdentity(claims, "Firebase");
                    context.User = new ClaimsPrincipal(identity);
                }
                catch (FirebaseAuthException ex)
                {
                    _logger.LogWarning($"Firebase token validation failed: {ex.Message}");
                }
            }

            await _next(context);
        }

        private string? ExtractToken(IHeaderDictionary headers)
        {
            var authHeader = headers["Authorization"].ToString();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return authHeader.Substring("Bearer ".Length);
            }
            return null;
        }
    }
}