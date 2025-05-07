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
        public HolidayController(IHolidayQuotaRepository holidayQuotaRepository, IHolidayRepository holidayRepository, ILeaveRepository leaveRepository, IEmployeeRepository employeeRepository)
        {
            _holidayQuotaRepository = holidayQuotaRepository;
            _holidayRepository = holidayRepository;
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        // View for holiday admin functions
        [HttpGet]
        public IActionResult HolidayManagement()
        {

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetAllQuotas(int year)
        {
            var idColumn = "Year";
            var quotas = await _holidayQuotaRepository.GetQuotasByYearAsync(idColumn,year);

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
                // 🟡 Step 1: Get full existing leave record
                var existing = await _leaveRepository.GetLeaveByIdAsync(idColumn,dto.LeaveId);
                if (existing == null)
                    return NotFound("Leave request not found.");

                // 🟢 Step 2: Only update status
                existing.Status = dto.Status;

                // 🟢 Step 3: Perform update
            
                var updated = await _leaveRepository.UpdateAsync(idColumn, existing);

                return Ok("Status updated.");
            }
            catch (Exception ex)
            {
                return BadRequest("Update failed.");
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























        //var sql = @"
        //    IF EXISTS (SELECT 1 FROM EmployeeHolidayQuota WHERE EmployeeID = @EmployeeID AND Year = @Year)
        //        UPDATE EmployeeHolidayQuota 
        //        SET TotalHolidays = @TotalHolidays 
        //        WHERE EmployeeID = @EmployeeID AND Year = @Year
        //    ELSE
        //        INSERT INTO EmployeeHolidayQuota (EmployeeID, Year, TotalHolidays)
        //        VALUES (@EmployeeID, @Year, @TotalHolidays)";













        // 1. Set holiday quota for an employee in a year
        //[HttpPost]
        //public async Task<IActionResult> SetQuota([FromBody] EmployeeHolidayQuota request)
        //{

        //    var userSession = SessionHelper.GetUserSession(HttpContext);
        //    if (userSession == null || !userSession.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return RedirectToAction("Index", "Home", new { area = "" });
        //    }

        //    try 
        //    {

        //        if (!ModelState.IsValid)
        //        {
        //            TempData["ErrorMessage"] = "Please correct the errors.";
        //            //await LoadEmployeeList(excludeUsers: true);
        //            return View(request);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return BadRequest("An error occurred while processing your request.");
        //    }





        //    var sql = @"
        //        IF EXISTS (SELECT 1 FROM EmployeeHolidayQuota WHERE EmployeeID = @EmployeeID AND Year = @Year)
        //            UPDATE EmployeeHolidayQuota 
        //            SET TotalHolidays = @TotalHolidays 
        //            WHERE EmployeeID = @EmployeeID AND Year = @Year
        //        ELSE
        //            INSERT INTO EmployeeHolidayQuota (EmployeeID, Year, TotalHolidays)
        //            VALUES (@EmployeeID, @Year, @TotalHolidays)";

        //    await _db.ExecuteAsync(sql, request);
        //    return Ok("Holiday quota set successfully.");
        //}

        //// 2. Approve a leave request
        //[HttpPost]
        //public async Task<IActionResult> ApproveLeave(int leaveId)
        //{
        //    var sql = "UPDATE LeaveRequests SET Status = 'Approved' WHERE LeaveID = @LeaveID";
        //    var rows = await _db.ExecuteAsync(sql, new { LeaveID = leaveId });

        //    if (rows > 0)
        //        return Ok("Leave approved.");
        //    else
        //        return NotFound("Leave ID not found.");
        //}

        // 3. Set holidays for the year: Sundays + festivals



        //[HttpPost]
        //public async Task<IActionResult> SetHolidays(int year)
        //{
        //    var holidays = new List<dynamic>();

        //    // Add Sundays
        //    var date = new DateTime(year, 1, 1);
        //    while (date.Year == year)
        //    {
        //        if (date.DayOfWeek == DayOfWeek.Sunday)
        //        {
        //            holidays.Add(new
        //            {
        //                HolidayName = "Sunday",
        //                HolidayDate = date,
        //                Description = "Weekly Off"
        //            });
        //        }
        //        date = date.AddDays(1);
        //    }

        //    // Add general/festival holidays
        //    holidays.AddRange(new[]
        //    {
        //        new { HolidayName = "New Year", HolidayDate = new DateTime(year, 1, 1), Description = "New Year Day" },
        //        new { HolidayName = "Republic Day", HolidayDate = new DateTime(year, 1, 26), Description = "National Holiday" },
        //        new { HolidayName = "Independence Day", HolidayDate = new DateTime(year, 8, 15), Description = "National Holiday" },
        //        new { HolidayName = "Gandhi Jayanti", HolidayDate = new DateTime(year, 10, 2), Description = "National Holiday" },
        //        new { HolidayName = "Christmas", HolidayDate = new DateTime(year, 12, 25), Description = "Christmas Day" }
        //    });

        //    var insertSql = @"
        //        IF NOT EXISTS (SELECT 1 FROM Holidays WHERE HolidayDate = @HolidayDate)
        //        BEGIN
        //            INSERT INTO Holidays (HolidayName, HolidayDate, Description)
        //            VALUES (@HolidayName, @HolidayDate, @Description)
        //        END";

        //    foreach (var h in holidays)
        //    {
        //        await _db.ExecuteAsync(insertSql, h);
        //    }

        //    return Ok("Holidays set for the year.");
        //}

        // DTO for setting quota

        //public class EmployeeHolidayQuota
        //{
        //    public int EmployeeID { get; set; }
        //    public int Year { get; set; }
        //    public int TotalHolidays { get; set; }
        //}

    }
}









//using AMS.Models;
//using System.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Dapper;

//namespace AMS.Controllers
//{
//    public class HolidayController : Controller
//    {
//        private readonly IDbConnection _db;

//        public HolidayController(IConfiguration config)
//        {
//            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
//        }

//        public async Task<IActionResult> Index()
//        {
//            var holidays = await _db.QueryAsync<Holiday>("SELECT * FROM Holidays");
//            return View(holidays);
//        }

//        [HttpPost]
//        public async Task<IActionResult> SetQuota(int employeeId, int year, int totalHolidays)
//        {
//            var query = "INSERT INTO EmployeeHolidayQuota (EmployeeID, Year, TotalHolidays) VALUES (@EmployeeID, @Year, @TotalHolidays)";
//            await _db.ExecuteAsync(query, new { employeeId, year, totalHolidays });
//            return RedirectToAction("Index");
//        }


//        [HttpPost]
//        public async Task<IActionResult> ApproveLeave(int leaveId)
//        {
//            var query = "UPDATE LeaveRequests SET Status = 'Approved' WHERE LeaveID = @LeaveID";
//            await _db.ExecuteAsync(query, new { LeaveID = leaveId });
//            return RedirectToAction("Index");
//        }


//    }
//}
