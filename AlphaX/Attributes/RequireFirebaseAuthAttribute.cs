using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlphaX.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireFirebaseAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Items.ContainsKey("FirebaseUser"))
            {
                context.Result = new UnauthorizedResult();
            }

            await Task.CompletedTask;
        }
    }
}