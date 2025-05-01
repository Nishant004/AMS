using AMS.Models.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;


namespace AMS.Filters

{
    public class AuthGuardAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public AuthGuardAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(session))
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
                return;
            }

            var user = JsonSerializer.Deserialize<AuthSession>(session);
            if (user == null || !string.Equals(user.Role, _requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
