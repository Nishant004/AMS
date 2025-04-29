using AMS.Interfaces;
using AMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using AMS.Models.ViewModel;

namespace AMS.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var users = await _userRepository.GetAllAsync();
            return View(users.OrderByDescending(u => u.UserId));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadEmployeeList(excludeUsers: true);
            return View(new UserCreateViewModel());
        }

        //[HttpPost]
        //public async Task<IActionResult> Create(User user)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Please correct the errors.";
        //            await LoadEmployeeList(excludeUsers: true);
        //            return View(user);
        //        }

        //        var existingUser = await _userRepository.GetByUsernameAsync(user.Username);

        //        if (existingUser != null)
        //        {
        //            // Username exists -> generate a new one
        //            user.Username = await GenerateUniqueUsername(user.Username);
        //        }

        //        user.PasswordHash = HashPassword(user.PasswordHash);

        //        await _userRepository.AddAsync(user);

        //        TempData["SuccessMessage"] = "User created successfully!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = $"Error: {ex.Message}";
        //        await LoadEmployeeList(excludeUsers: true);
        //        return View(user);
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the errors.";
                    await LoadEmployeeList(excludeUsers: true);
                    return View(model);
                }

                var existingUser = await _userRepository.GetByUsernameAsync(model.Username);

                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = $"Username '{model.Username}' already exists.";
                    await LoadEmployeeList(excludeUsers: true);
                    return View(model);
                }

                var user = new User
                {
                    EmployeeId = model.EmployeeId,
                    Username = model.Username,
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
                await LoadEmployeeList(excludeUsers: true);
                return View(model);
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


        private async Task LoadEmployeeList(bool excludeUsers = false)
        {
            IEnumerable<Employees> employees;

            if (excludeUsers)
            {
                // Get employees who are not already assigned to a user
                var userEmployeeIds = (await _userRepository.GetAllAsync())
                    .Where(u => u.EmployeeId.HasValue)
                    .Select(u => u.EmployeeId.Value)
                    .ToList();

                employees = await _employeeRepository.GetAllAsync();

                // Filter out employees that are already in the User table
                employees = employees.Where(e => !userEmployeeIds.Contains(e.EmployeeId)).ToList();
            }
            else
            {
                // Get all employees without filtering
                employees = await _employeeRepository.GetAllAsync();
            }

            ViewBag.EmployeeList = new SelectList(employees, "EmployeeId", "FirstName");
        }

    }
}
