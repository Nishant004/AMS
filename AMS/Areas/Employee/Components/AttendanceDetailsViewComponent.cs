using AMS.Helpers;
using AMS.Interfaces;
using AMS.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AMS.Areas.Employee.Components
{
    public class AttendanceDetailsViewComponent : ViewComponent
    {
        private readonly IEmployeeRepository _employeeRepository;

        public AttendanceDetailsViewComponent(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()

        {

            var userSession = SessionHelper.GetUserSession(HttpContext);
            if (userSession == null || !userSession.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return View("Default", new EmployeeAttendanceDto()); ;
            }



            var attendance = await _employeeRepository.GetEmployeeAttendanceByDateAsync(userSession.EmployeeId);
            return View("Default", attendance ?? new EmployeeAttendanceDto());
        }
    }
}
