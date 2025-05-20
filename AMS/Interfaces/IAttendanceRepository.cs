using AMS.Models;

namespace AMS.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<Attendance> GetByEmployeeAndDateAsync(int employeeId, DateTime date);

        Task<int> UpdateAsync(string idColumn, Attendance dto);

        public Task<int> InsertAsync(Attendance newAttendance);

    }
}
