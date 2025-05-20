using AMS.Filters;
using AMS.Helpers;
using AMS.Interfaces;
using AMS.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;

namespace AMS.Areas.Employee.Controllers
{
    [Area("Employee")]
    [AuthGuard("Employee")]
    public class LeaveController : Controller
    {
        private readonly ILeaveRepository _leaveRepository;

        public LeaveController(ILeaveRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }

        public async Task<IActionResult> Index()
        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var idColumn = "EmployeeId";
            var Id = userSession.EmployeeId;

            var leaves = await _leaveRepository.GetLeavesByEmployeeAsync(idColumn, Id);
            return View(leaves); // Show a list of submitted leave requests



        }

        [HttpGet]
        public IActionResult RequestLeave()
        {
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var model = new LeaveRequests
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date,
                EmployeeID = userSession.EmployeeId
            };



            return View(model); // Renders the RequestLeave.cshtml form
        }

        [HttpPost]
        public async Task<IActionResult> RequestLeave(LeaveRequests leave)
        {
            if (!ModelState.IsValid)
            {
                return View(leave);
            }

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            leave.EmployeeID = userSession.EmployeeId;
            leave.Status = "Pending";

            await _leaveRepository.CreateLeaveRequestAsync(leave);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var idColumn = "LeaveId";
            var leave = await _leaveRepository.GetLeaveByIdAsync(idColumn, id);
            if (leave == null || leave.EmployeeID != userSession.EmployeeId)
            {
                TempData["ErrorMessage"] = "Leave request not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            var deleted = await _leaveRepository.DeleteLeaveRequestAsync(idColumn , id);
            if (deleted == 1 )
            {
                TempData["SuccessMessage"] = "Leave request deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the leave request.";
            }

            return RedirectToAction(nameof(Index));
        }



    }
}




