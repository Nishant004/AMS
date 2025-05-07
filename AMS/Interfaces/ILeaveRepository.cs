using AMS.Models;
using AMS.Models.ViewModel;

namespace AMS.Interfaces
{
    public interface ILeaveRepository
    {

        Task<IEnumerable<LeaveRequests>> GetLeavesByEmployeeAsync(string idColumn, int Id);

        Task<IEnumerable<LeaveRequests>> GetLeavesByEmployeeAndMonthAsync(int employeeId, int year);


        Task<int> CreateLeaveRequestAsync(LeaveRequests leave);

        Task<LeaveRequests> GetLeaveByIdAsync(string idColumn, int id);

        Task<int> DeleteLeaveRequestAsync(string idColumn , int id);

        Task<IEnumerable<LeaveRequests>> GetAllLeavesAsync();

        Task<int> UpdateAsync(string idColumn, LeaveRequests dto);


    }



}
