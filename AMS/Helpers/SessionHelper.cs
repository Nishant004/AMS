using AMS.Models.ServiceModels;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AMS.Helpers // You can change this namespace to match your project structure
{
    public static class SessionHelper
    {
        public static AuthSession? GetUserSession(HttpContext httpContext)
        {
            var sessionData = httpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(sessionData))
                return null;

            return JsonSerializer.Deserialize<AuthSession>(sessionData);
        }
    }
}
