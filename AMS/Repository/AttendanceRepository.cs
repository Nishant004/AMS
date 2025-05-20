using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AMS.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {

        private readonly IGenericRepository<Attendance> _attendanceRepository;

        public AttendanceRepository(IGenericRepository<Attendance> attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Attendance> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            return await _attendanceRepository.GetAttendanceByEmployeeDateAsync(employeeId, date);
        }

        public async Task<int> UpdateAsync(string idColumn, Attendance dto)
        {
            return await _attendanceRepository.UpdateAsync(idColumn, dto);
        }

        public Task<int> InsertAsync(Attendance newAttendance)
        {
            return _attendanceRepository.InsertAsync(newAttendance);

        }
    }
}
