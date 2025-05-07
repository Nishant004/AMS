using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using AMS.Models.ViewModel;
using Humanizer;

namespace AMS.Repository
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly IGenericRepository<LeaveRequests> _leaveGenRepository;


        public LeaveRepository(IGenericRepository<LeaveRequests> leaveGenRepository)

        {

            _leaveGenRepository = leaveGenRepository;

        }

        public async Task<IEnumerable<LeaveRequests>> GetLeavesByEmployeeAsync(string idColumn, int Id)

        {
            //return await _leaveGenRepository.GetByIdAsync(idColumn, id);
            return await _leaveGenRepository.GetAttendanceByIdAsync(idColumn, Id);
        }



        public async Task<IEnumerable<LeaveRequests>> GetLeavesByEmployeeAndMonthAsync(int employeeId, int year)
        {

            return await _leaveGenRepository.GetLeavesByEmployeeAndMonthAsync(employeeId, year);



        }

        public async Task<int> CreateLeaveRequestAsync(LeaveRequests leave)

        {
            return await _leaveGenRepository.CreateLeaveRequestAsync(leave);


        }



        public async Task<LeaveRequests> GetLeaveByIdAsync(string idColumn, int id)
        {


            return await _leaveGenRepository.GetByIdAsync(idColumn, id);

        }

        public async Task<int> DeleteLeaveRequestAsync(string idColumn, int id)
        {

            return await _leaveGenRepository.DeleteAsyncPermanent(idColumn, id);



        }

        public Task<IEnumerable<LeaveRequests>> GetAllLeavesAsync()
        {
            return _leaveGenRepository.GetAllAsync();
        }

        public async Task<int> UpdateAsync(string idColumn, LeaveRequests newStatus)
        {
            return await _leaveGenRepository.UpdateAsync(idColumn, newStatus);
        }
    }
}