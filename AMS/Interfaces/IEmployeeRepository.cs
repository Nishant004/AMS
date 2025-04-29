using AMS.Models;
using AMS.Models.ViewModel;

namespace AMS.Interfaces
{
    public interface IEmployeeRepository
    {

        Task<bool> CheckInAsync(int employeeId, string ip, double? checkInLat, double? checkInLong, string followUpShift);


        //not in use 
        //Task<int> AddAttendanceAsync(Attendance attendance);
        // till here




        Task<Attendance?> GetAttendanceByEmployeeDateAsync(int employeeId, DateTime date);
        Task UpdateAttendanceAsync(Attendance attendance);

        //DATA FETCH FROM USER
        Task<EmployeeAttendanceDto?> GetEmployeeAttendanceByDateAsync(int employeeId);


        Task LogCheckOutAsync(int attendanceId, TimeSpan checkInTime, TimeSpan checkOutTime, double? checkInLat, double? checkInLong, double? checkOutLat, double? checkOutLong);



        Task<IEnumerable<AttendanceLogDto>> GetAttendanceLogsAsync(int employeeId, int year, int month, int day);


        Task<IEnumerable<EmpAttendanceDto>> GetAttendanceByMonthYearAsyncById(int employeeId, int month, int year);


        Task<IEnumerable<Employees>> GetAllAsync();


        Task<Employees?> GetByIdAsync(string idColumn, int id); // Get by ID
    }
}
