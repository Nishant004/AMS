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
                // 🟡 Step 1: Get full existing leave record
                var existing = await _leaveRepository.GetLeaveByIdAsync(idColumn, dto.LeaveId);
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



        //[HttpPost]
        //public async Task<IActionResult> AddSundays(int year)
        //{

        //    if (year < 2000 || year > 2100)
        //        return BadRequest("Invalid year.");

        //    var count = await _holidayRepository.AddSundaysAsync(year);
        //    return Ok($"{count} Sundays added as holidays for {year}.");

        //}

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