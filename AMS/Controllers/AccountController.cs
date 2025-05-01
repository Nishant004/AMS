using Microsoft.AspNetCore.Mvc;
using AMS.Interfaces;
using AMS.Models.ViewModel;
using AMS.Models.ServiceModels;




namespace AMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthService _authService;

        public AccountController(IUserRepository userRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

     

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);
            try
            {

                // 1. Get user from DB
                var user = await _userRepository.GetByUsernameAsync(model.Username);


                //Role check

                //if (!string.Equals(user.Role, model.Role, StringComparison.OrdinalIgnoreCase))

                    if (!user.Role.Equals(model.Role, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Incorrect Role.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }



                if (user == null)
                {
                    TempData["Error"] = "Invalid username";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                //Console.WriteLine($"Raw password: {model.PasswordHash}");
                //Console.WriteLine($"Stored hash: {user.PasswordHash}");

                // 2. Verify password

                bool isVerified = BCrypt.Net.BCrypt.Verify(model.PasswordHash, user.PasswordHash);
                if (!isVerified)
                {
                    TempData["Error"] = "Incorrect password.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                Console.WriteLine($"User found: {user.Username}, Role: {user.Role}");


                // 2. Save session
                var session = new AuthSession
                {
                    UserId = user.UserId,
                    Name = user.Username,
                    Role = user.Role,
                    EmployeeId = user.EmployeeId
                };

                await _authService.LoginAsync(session);

                // 3. Redirect based on role
                if (user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Attendance", new { area = "" });
                }

                else if (user.Role.Equals("employee", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Employee", new { area = "Employee" });
                }

                TempData["Error"] = "Unknown role.";
                return RedirectToAction("Index", "Home", new { area = "" });

            }


            catch (Exception ex)
            {
                // Optional: log ex.Message or ex.ToString() for debugging
                TempData["Error"] = "An error occurred during login. Please try again.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }


        }

        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
