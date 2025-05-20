using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;
using AMS.Models;
using AMS.Filters;
using AMS.Data;
using AMS.Interfaces;
using AMS.Helpers;
using System.Linq.Expressions;
using AMS.Models.ViewModel;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace YourNamespace.Controllers
{
    [Area("Admin")]
    [AuthGuard("Admin")]
    public class HolidayController : Controller
    {

        private readonly IHolidayQuotaRepository _holidayQuotaRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        public HolidayController(IHolidayQuotaRepository holidayQuotaRepository, IHolidayRepository holidayRepository, ILeaveRepository leaveRepository, IEmployeeRepository employeeRepository , IAttendanceRepository attendanceRepository)
        {
            _holidayQuotaRepository = holidayQuotaRepository;
            _holidayRepository = holidayRepository;
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
            _attendanceRepository = attendanceRepository;
        }

        [HttpGet]
        public IActionResult HolidayManagement()
        {

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetAllQuotas(int year)
        {
            var idColumn = "Year";
            var quotas = await _holidayQuotaRepository.GetQuotasByYearAsync(idColumn, year);

            var employees = await _employeeRepository.GetAllAsync();

            var result = (from q in quotas
                          join e in employees on q.EmployeeID equals e.EmployeeId
                          select new EmployeeQuotaDisplayVM
                          {
                              EmployeeID = q.EmployeeID,
                              FullName = e.FirstName + " " + e.LastName,
                              Year = q.Year,
                              TotalHolidays = q.TotalHolidays
                          }).ToList();


            return PartialView("_QuotaTable", result);
        }


        [HttpPost]
        public async Task<IActionResult> SetQuotaForAll([FromBody] BulkQuotaRequestDto request)
        {
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            try
            {


                //var employees = await _db.QueryAsync<int>("SELECT EmployeeID FROM Employees");

                var employees = await _employeeRepository.GetAllAsync();



                var allEmp = employees.Select(e => e.EmployeeId).ToList();

                //var checkEmpYear = await _holidayQuotaRepository.get


                await _holidayQuotaRepository.ApplyQuotaAsync(allEmp, request.Year, request.TotalHolidays);


                //var applyQuota = await _holidayQuotaRepository.ApplyQuota(list(allEmp));

                return Ok("Holiday quota set for all employees.");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while setting quota for all employees.");
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetPendingLeaves()
        {
            var leaves = await _leaveRepository.GetAllLeavesAsync();

            var pending = leaves.Where(l => l.Status == "Pending").ToList();

            return PartialView("_PendingLeaves", pending);
        }



       

        [HttpPost]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] LeaveStatusUpdateDto dto)
        {
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            try
            {
                var idColumn = "LeaveId";

                // 🔹 Step 1: Get existing leave
                var existing = await _leaveRepository.GetLeaveByIdAsync(idColumn, dto.LeaveId);
        
                if (existing == null)
                    return NotFound("Leave request not found.");

                // 🔹 Step 2: Update status
                existing.Status = dto.Status;
                var updated = await _leaveRepository.UpdateAsync(idColumn, existing);


                if (dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    var startDate = Convert.ToDateTime(existing.StartDate);
                    var endDate = Convert.ToDateTime(existing.EndDate);

                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        if (await _holidayRepository.ExistsAsync(date))
                        {
                            continue; // Don't update or insert attendance on holidays
                        }

                        // Proceed with attendance logic
                        var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(existing.EmployeeID, date);

                        if (attendance != null)
                        {
                            // Update existing attendance
                            attendance.Status = "Leave";
                            attendance.LeaveID = existing.LeaveID; // ✅ Link to LeaveRequest
                            attendance.AttendanceDate = date; // Ensure the date is set correctly
                            attendance.EmployeeID = existing.EmployeeID;
                            attendance.CheckInTime = null;
                            await _attendanceRepository.UpdateAsync("AttendanceID", attendance);
                        }
                        else
                        {
                            // Insert new attendance entry marked as Leave
                            var newAttendance = new Attendance
                            {
                                EmployeeID = existing.EmployeeID,
                                AttendanceDate = date,
                                Status = "Leave",
                                CheckInTime = null,
                                LeaveID = existing.LeaveID // ✅ Link to LeaveRequest
                            };

                        await _attendanceRepository.InsertAsync(newAttendance);
                        }
                    }
                }





                // ✅ Triggers handle Attendance update/insertion automatically
                return Ok("Leave status updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Update failed: " + ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetApprovedLeaves()
        {
            var leaves = await _leaveRepository.GetAllLeavesAsync();

            var pending = leaves.Where(l => l.Status == "Approved").ToList();

            return PartialView("_ApprovedLeaves", pending);
        }


        [HttpGet]
        public async Task<IActionResult> GetRejectedLeaves()
        {
            var leaves = await _leaveRepository.GetAllLeavesAsync();

            var pending = leaves.Where(l => l.Status == "Rejected").ToList();

            return PartialView("_ApprovedLeaves", pending);
        }



        //holiday incert



        [HttpPost]
        public async Task<IActionResult> AddHoliday([FromBody] Holidays model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data. Please check the form.");

            model.HolidayDate = model.HolidayDate.Date; // Normalize

            if (await _holidayRepository.ExistsAsync(model.HolidayDate))
                return BadRequest("Holiday already exists on this date.");

            await _holidayRepository.AddHolidayAsync(model);
            return Ok("Holiday added successfully.");
        }






        [HttpGet]
        public IActionResult AddHolidayForm()
        {
            return PartialView("_AddHolidayPartial", new Holidays());
        }


        [HttpPost]
        [Route("Admin/Holiday/AddSundays/{year}")]
        public async Task<IActionResult> AddSundays(int year)
        {
            try
            {

                if (year < 2000 || year > 2100)
                    return BadRequest("Invalid year.");

                var count = await _holidayRepository.AddSundaysAsync(year);
                return Ok($"{count} Sundays added as holidays for {year}.");

            }
            catch (Exception ex)
            {
                return BadRequest("Failed to add Sundays: " + ex.Message);
            }
        }



        public async Task<IActionResult> GetAllHolidays()
        {
            var holidays = await _holidayRepository.GetAllHolidays();

            if (holidays == null || !holidays.Any())
            {
                holidays = new List<Holidays>(); // ensures the view won't break
            }

            var sortedHolidays = holidays.OrderBy(h => h.HolidayDate).ToList();

            return PartialView("_HolidayListPartial", sortedHolidays);
        }



    }
}







//public async Task<Attendance> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
//{
//    var sql = @"
//        SELECT TOP 1 *
//        FROM Attendance
//        WHERE EmployeeID = @EmployeeID AND CAST(AttendanceDate AS DATE) = @Date";

//    using var connection = _context.CreateConnection();
//    return await connection.QueryFirstOrDefaultAsync<Attendance>(sql, new
//    {
//        EmployeeID = employeeId,
//        Date = date.Date
//    });
//}


//ALTER TABLE Attendance
//ADD LeaveID INT NULL;

//ALTER TABLE Attendance
//ADD CONSTRAINT FK_Attendance_LeaveRequests
//FOREIGN KEY (LeaveID) REFERENCES LeaveRequests(LeaveID)
//ON DELETE CASCADE;


//INSERT INTO Attendance (EmployeeID, AttendanceDate, Status, Remarks, LeaveID)
//VALUES (@EmployeeID, @Date, 'Leave', 'Auto-generated from leave', @LeaveID);

//CREATE TRIGGER trg_DeleteAttendanceOnLeaveDelete
//ON LeaveRequests
//FOR DELETE
//AS
//BEGIN
//    -- Delete the related attendance records
//    DELETE FROM Attendance
//    WHERE LeaveID IN (SELECT LeaveID FROM deleted);
//END;


