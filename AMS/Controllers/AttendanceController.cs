using AMS.Interfaces;
using AMS.Models;
using AMS.Models.ViewModel;
using AMS.Repository;
using AMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AMS.Helpers;

namespace AMS.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAdminRepository _adminRepository;

        public AttendanceController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> Index()
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }


         

            var employee = await _adminRepository.GetAllAsync();

            Console.WriteLine("show it: " +employee);


            int totalEmployees = employee.Count();

            string idColumn = "AttendanceDate";
            var attendence = await _adminRepository.GetAttendanceByIdAsync(idColumn, DateTime.Today);

            int presentToday = 0;
            int absentToday = 0;
            int onLeave = 0;

            foreach (var item in attendence)
            {
                if (item.Status == "Present" | item.Status == "present")
                {
                    presentToday++;
                }
                if (item.Status == "Absent" | item.Status == "absent")
                {
                    absentToday++;
                }
                if (item.Status == "Leave" | item.Status == "leave")
                {
                    onLeave++;
                }

            }

            var employeeStatus = new EmployeeStatusViewModel
            {
                TotalEmployees = totalEmployees,
                PresentToday = presentToday,
                AbsentToday = absentToday,
                OnLeave = onLeave
            };


            return View(employeeStatus);
        }

        public async Task<IActionResult> GetEmployees()
        {
            Console.WriteLine("GetEmployees Controller called");

            var employee = await _adminRepository.GetAllAsync();

            var result = employee.Select(e => new
            {
                id = e.EmployeeId,
                name = e.FirstName + " " + e.LastName
            });

            return Json(result);
        }

        public async Task<IActionResult> GetEmployeeAttendance(int employee, int month, int year)
        {
            Console.WriteLine("GetEmployeeAttendance Controller called");

            var employeeDetails = await _adminRepository.GetAllAsync();

            var employeeResult = employeeDetails.Select(e => new
            {
                id = e.EmployeeId,
                name = e.FirstName + " " + e.LastName
            }).ToList();

            var result = await _adminRepository.GetAttendanceByMonthYearAsync(employee, month, year);

            // Join attendance with employee name
            var enrichedResult = result.Select(att =>
            {
                var emp = employeeResult.FirstOrDefault(e => e.id == att.EmployeeID);
                return new
                {
                    employeeID = att.EmployeeID,
                    employeeName = emp != null ? emp.name : "Unknown",
                    attendanceDate = att.AttendanceDate,
                    status = att.Status
                };
            });

            return new JsonResult(enrichedResult);
        }

    }
}


