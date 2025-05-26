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
        private readonly IEmployeeRepository _employeeRepository;

        public AccountController(IUserRepository userRepository, AuthService authService,IEmployeeRepository employeeRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _employeeRepository = employeeRepository;
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

                if (user == null)
                {
                    TempData["Error"] = "Invalid username";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                

                int id = user.EmployeeId;
                string idColumn = "EmployeeId";
                var checkActive = await _employeeRepository.GetByIdAsync(idColumn,id);

              

                if (checkActive?.Status == "Inactive")
                {
                    TempData["Error"] = "Your account is deactivated. Please contact the administrator.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }


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

               

                // 2. Verify password

                bool isVerified = BCrypt.Net.BCrypt.Verify(model.PasswordHash, user.PasswordHash);
                if (!isVerified)
                {
                    TempData["Error"] = "Incorrect password.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

               


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
