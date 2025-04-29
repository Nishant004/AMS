//using Newtonsoft.Json;
//using CRM.Data;
//using CRM.Models.ServiceModels;
//using System.Security.Claims;

//namespace CRM.Services
//{
//    public class AuthService
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public AuthService(IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//        }

//        public async Task LoginAsync(AuthSession session)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, session.Name),
//                new Claim(ClaimTypes.Role, session.Role)
//            };

//            _httpContextAccessor.HttpContext.Session.SetString("User", JsonConvert.SerializeObject(session));
//        }

//        public Task LogoutAsync()
//        {
//            _httpContextAccessor.HttpContext.Session.Clear();
//            return Task.CompletedTask;
//        }
//    }
//}
