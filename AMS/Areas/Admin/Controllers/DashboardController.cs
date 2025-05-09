using System.Runtime.InteropServices;
using System.Text.Json;
using AMS.Interfaces;
using AMS.Models;
using AMS.Models.ViewModel;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Helpers;
using QuestPDF.Fluent;
using AMS.Models.ServiceModels;
using AMS.Filters;

namespace AMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthGuard("Admin")]
    public class DashboardController : Controller
    {
        private readonly IAdminRepository _adminRepository;
        private readonly PdfService _pdfService;
        private readonly IViewRenderService _viewRenderService;


        public DashboardController(IAdminRepository adminRepository, PdfService pdfservice, IViewRenderService viewRenderService)
        {
            _adminRepository = adminRepository;
            _pdfService = pdfservice;
            _viewRenderService = viewRenderService;
        }


        public async Task<IActionResult> Index()
        {
            var employee = await _adminRepository.GetAllAsync();
            return View(employee);
        }




        public IActionResult Create()
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create([Bind("FirstName, LastName, Email, PhoneNumber, Department, Designation, JoiningDate, Status,Project")] Employees employee)
        {
            Console.WriteLine("Create Post called");


     

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            if (ModelState.IsValid)
            {
                

                @employee.JoiningDate.ToString("dd/MM/yyyy");

                Console.WriteLine("Date: " + employee.JoiningDate);

                var result = await _adminRepository.InsertAsync(employee);

                if (result != null)
                {
                    TempData["Notification"] = "Employee Added Successfully";
                    TempData["NotificationType"] = "success";
                }

                return RedirectToAction("Index");
            }
            // Debugging Required fields
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Create: ModelState is NOT valid!");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }
                return View(employee);
            }
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
        

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            string idColumn = "EmployeeId";
            var employee = await _adminRepository.GetByIdAsync(idColumn, id);

            if (employee == null)
            {
                return NotFound();
            }
            else
            {
                return View(employee);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("EmployeeId, FirstName, LastName, Email, PhoneNumber, Department, Designation, JoiningDate, Status,Project")] Employees employee)
        {
            

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            if (ModelState.IsValid)
            {

                string idColumn = "EmployeeId";
                var existingEmployee = await _adminRepository.GetByIdAsync(idColumn, employee.EmployeeId);

                if (existingEmployee == null)
                {
                    return NotFound();
                }

                //var query1 = "UPDATE Products SET Name = @Name, Price = @Price, Description = @Description, Stock = @Stock WHERE Id = @Id";
                //await _db.ExecuteAsync(query1, product);

                var result = await _adminRepository.UpdateAsync(idColumn, employee);

                if (result != null)
                {
                    TempData["Notification"] = "Employ Updated Successfully";
                    TempData["NotificationType"] = "success";
                }

                return RedirectToAction("Index");
            }

            // Required Field Debugging
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Edit: ModelState is NOT valid!");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }
                return View(employee);
            }

            return View(employee);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            string idColumn = "EmployeeId";
            var existingEmployee = await _adminRepository.GetByIdAsync(idColumn, id);

            Console.WriteLine("GET: ID Value: " + id);
            Console.WriteLine("GET: existingEmployee: " + existingEmployee);

            if (existingEmployee == null)
            {
                return NotFound();
            }
            return View(existingEmployee);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int EmployeeId)
        {
            Console.WriteLine("ConfirmDelete Called");

       
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }




            string idColumn = "EmployeeId";
            var existingEmployee = await _adminRepository.GetByIdAsync(idColumn, EmployeeId);

            Console.WriteLine("ID Value: " + EmployeeId);
            Console.WriteLine("existingEmployee: " + existingEmployee);

            if (existingEmployee != null)
            {
                Console.WriteLine("existingEmployee Called");

                var result = await _adminRepository.DeleteAsync(idColumn, EmployeeId);

                if (result != null)
                {
                    TempData["Notification"] = "Employee Deleted Successfully";
                    TempData["NotificationType"] = "success";
                }

            }
            return RedirectToAction("Index");
        }




        public async Task<IActionResult> EmployeeDetails(int id, int? month, int? year)
        {
            

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            string idColumn = "EmployeeId";

            var employee = await _adminRepository.GetByIdAsync(idColumn, id);
            var attendence = await _adminRepository.GetAttendanceByIdAsync(idColumn, id);

            if (month.HasValue && year.HasValue)
            {
                attendence = attendence
                    .Where(a => a.AttendanceDate.Month == month.Value && a.AttendanceDate.Year == year.Value)
                    .ToList();
            }

            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                AttendanceRecord = attendence.ToList()
            };

            return View(viewModel);

          
        }

     

        public async Task<IActionResult> DownloadQuestPdf(int id, int month, int year)
        {
            string idColumn = "EmployeeId";

     

            var employee = await _adminRepository.GetByIdAsync(idColumn, id);
            var attendance = await _adminRepository.GetAttendanceByIdAsync(idColumn, id);



            // Filter attendance for the given month and year
            attendance = attendance
                .Where(a => a.AttendanceDate.Month == month && a.AttendanceDate.Year == year)
                .ToList();

            var model = new EmployeeDetailsViewModel
            {
                Employee = employee,
                AttendanceRecord = (List<Attendance>)attendance
            };

            var document = new EmployeeDetailsDocument(model);
            var pdfBytes = document.GeneratePdf();

            // Format filename: AnkitApril25.pdf
            var date = new DateTime(year, month, 1);
            var monthName = date.ToString("MMMM");
            var yearTwoDigit = date.ToString("yy");
            var firstName = employee?.FirstName?.Replace(" ", "") ?? "Employee";
            var lastName = employee?.LastName?.Replace(" ", "") ?? "";

            var fullName = $"{firstName}{lastName}";
            var fileName = $"{fullName}{monthName}{yearTwoDigit}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }


        public IActionResult Test()
        {
            Console.WriteLine("Test Called");
            return View();
        }

    }
}
