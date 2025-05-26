using AMS.Filters;
using AMS.Helpers;
using AMS.Interfaces;
using AMS.Models.ViewModel;
using AMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthGuard("Admin")]
    public class AbsentUpdateController : Controller
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IAttendanceRepository _attendanceRepository;

        public AbsentUpdateController(IAdminRepository adminRepository, IAttendanceRepository attendanceRepository)
        {
            _adminRepository = adminRepository;
            _attendanceRepository = attendanceRepository;
        }


        [HttpGet]
        public async Task<IActionResult> EmployeeAttendance(DateTime? date, string status)
        {
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            var employees = await _adminRepository.GetAllAsync(); // Get all employees
            var attendanceList = await _adminRepository.GetAttendanceByIdAsync("AttendanceDate", date ?? DateTime.Today);

          

            var result = (from e in employees
                          join a in attendanceList on e.EmployeeId equals a.EmployeeID into ea
                          from a in ea.DefaultIfEmpty() // left join
                          where a == null || a.Status == null // show only if no record or null status
                          select new EmployeeAttendanceVM
                          {
                              EmployeeId = e.EmployeeId,
                              FirstName = e.FirstName,
                              LastName = e.LastName,
                              Status = a?.Status, // will be null
                              AttendanceDate = a?.AttendanceDate
                          }).ToList();

            return View(result);
        }





        public async Task<IActionResult> EmployeeAttendance(DateTime date, string status, List<int> employeeIds, int? leaveId)
        {
            if (employeeIds == null || !employeeIds.Any())
            {
                TempData["Error"] = "No employees selected.";
                return RedirectToAction("EmployeeAttendance", new { date = date.ToString("yyyy-MM-dd"), status });
            }

            foreach (var empId in employeeIds)
            {
                var attendanceDto = new Attendance
                {
                    EmployeeID = empId,
                    AttendanceDate = date,
                    Status = status,
                    LeaveID = (status == "Leave" && leaveId.HasValue) ? leaveId : null // Only set LeaveID if it's relevant
                };

                // Insert attendance record using your repository or insert logic
                await _attendanceRepository.InsertAsync(attendanceDto);
            }

            TempData["Success"] = $"Marked '{status}' for {employeeIds.Count} employee(s) on {date:yyyy-MM-dd}.";
            return RedirectToAction("EmployeeAttendance", new { date = date.ToString("yyyy-MM-dd"), status });
        }



    }
}
