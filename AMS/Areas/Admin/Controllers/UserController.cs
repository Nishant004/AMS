using AMS.Interfaces;
using AMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using AMS.Models.ViewModel;
using AMS.Filters;
using AMS.Helpers;

namespace AMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthGuard("Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;


        public UserController(IUserRepository userRepository, IEmployeeRepository employeeRepository)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<IActionResult> Index()
        {

            var employees = (await _userRepository.GetAllAsync())
            .Where(e => e.Role.Equals("employee", StringComparison.OrdinalIgnoreCase))
            .ToList();


            return View(employees.OrderByDescending(e => e.EmployeeId));

           
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //await LoadEmployeeList(excludeUsers: true);
            await LoadEmployeeList();
            return View(new UserCreateViewModel());
        }

  


        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the errors.";
                    await LoadEmployeeList();
                    //await LoadEmployeeList(excludeUsers: true);
                    return View(model);
                }

                var existingUser = await _userRepository.GetByUsernameAsync(model.Username);

                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = $"Username '{model.Username}' already exists.";
                    //await LoadEmployeeList(excludeUsers: true);
                    await LoadEmployeeList();
                    return View(model);
                }


                var user = new User
                {
                    
                    Username = model.Username,
                    EmployeeId = model.EmployeeId,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };

                var userdetails= await _userRepository.AddAsync(user);


                Console.WriteLine($"User:{userdetails}");

                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                //await LoadEmployeeList(excludeUsers: true);
                await LoadEmployeeList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {


            var idColumn = "UserID";
            var user = await _userRepository.GetByIdAsync(idColumn, id);
            Console.WriteLine(id);
            if (user == null)
                return NotFound();

            return View(user); // This will return Delete.cshtml with the user model
        }



        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            try
            {
                var idColumn = "UserID";
                var user = await _userRepository.GetByIdAsync(idColumn, id);
                Console.WriteLine(id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _userRepository.DeleteAsync(idColumn,id);

                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }




        // Get the edit password form for a specific user
        [HttpGet]
        public async Task<IActionResult> EditPassword(int id)
        {
            var idColumn = "UserID";
            var user = await _userRepository.GetByIdAsync(idColumn, id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(user); // returns EditPassword.cshtml
        }


        // POST: Update the user's password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(int id, string newPassword)
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            try
            {
                var idColumn = "UserID";
                var user = await _userRepository.GetByIdAsync(idColumn, id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Hash the new password before saving
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // Update the password
                await _userRepository.UpdateAsync(idColumn,user);

                TempData["SuccessMessage"] = "Password updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }





        [HttpGet]
        public async Task<IActionResult> GenerateUniqueUsername(int employeeId)
        {


            // Get employee info by ID
            string idColumn = "EmployeeId";


            var employee = await _employeeRepository.GetByIdAsync(idColumn, employeeId);

            if (employee == null)
            {
                return BadRequest("Employee not found");
            }

            string baseUsername = employee.FirstName; // Use employee's first name
            string uniqueUsername = await GenerateUniqueUsername(baseUsername);

            return Ok(uniqueUsername);
        }



        private async Task<string> GenerateUniqueUsername(string baseUsername)
        {
            // Generate a random 3-digit number
            Random random = new Random();
            int randomNumber = random.Next(100, 999); // Generates a random number between 100 and 999

            string newUsername = $"{baseUsername}{randomNumber}";

            // Check if the new username exists in the database
            while (await _userRepository.GetByUsernameAsync(newUsername) != null)
            {
                // If username exists, generate a new random 3-digit number and append it again
                randomNumber = random.Next(100, 999);
                newUsername = $"{baseUsername}{randomNumber}";
            }

            return newUsername;
        }

        private async Task LoadEmployeeList()
        {
            // Step 1: Get all active employees
            var allEmployees = (await _employeeRepository.GetAllAsync())
                .Where(e => e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Step 2: Get users who are either 'employee' or 'admin'
            var userEmployeeIds = (await _userRepository.GetAllAsync())
                .Where(u =>
                    u.Role.Equals("employee", StringComparison.OrdinalIgnoreCase) || 
                    u.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                .Select(u => u.EmployeeId)
                .ToHashSet(); // HashSet for fast lookup

            // Step 3: Exclude all those EmployeeIds from employees
            var availableEmployees = allEmployees
                .Where(e => !userEmployeeIds.Contains(e.EmployeeId))
                .ToList();

            // Step 4: Bind to dropdown
            ViewBag.EmployeeList = new SelectList(
                availableEmployees.Select(e => new {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.LastName
                }),
                "EmployeeId", "FullName"
            );
        }



    }
}
