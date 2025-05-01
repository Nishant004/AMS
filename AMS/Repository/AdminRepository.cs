using AMS.Interfaces;
using AMS.Models;
using AMS.Data;
using NuGet.Protocol.Core.Types;
using static Dapper.SqlMapper;

namespace AMS.Repository
{
    public class AdminRepository : IAdminRepository
    {
        public IGenericRepository<Admin> _adminGenRepository { get; }
        public IGenericRepository<Employees> _employeeGenRepository { get; }
        public IGenericRepository<Attendance> _employeeAttendance { get; }
        public IGenericRepository<User> _user { get; }


        public AdminRepository(IGenericRepository<Admin> adminGenRepository, IGenericRepository<Employees> employeeGenRepository, IGenericRepository<Attendance> employeeAttendance, IGenericRepository<User> user)
        {
            _adminGenRepository = adminGenRepository;
            _employeeGenRepository = employeeGenRepository;
            _employeeAttendance = employeeAttendance;
            _user = user;
        }


   
        public Task<(User? user, bool isDeactivated)> GetByUserCredentialsAsync(
            string usernameColumn, string passwordColumn, string roleColumn,
            string username, string password, string role)
                {
                    return _user.GetByUserCredentialsAsync<User>(usernameColumn, passwordColumn, roleColumn, username, password, role);
                }




        // Get All Employees
        Task<IEnumerable<Employees>> IAdminRepository.GetAllAsync()
        {
            return _employeeGenRepository.GetAllAsync();
        }

        // Get Employee By Id
        Task<Employees> IAdminRepository.GetByIdAsync(string idColumn, int id)
        {
            return _employeeGenRepository.GetByIdAsync(idColumn, id);
        }

        // Insert employee
        Task<int> IAdminRepository.InsertAsync(Employees entity)
        {
            return _employeeGenRepository.InsertAsync(entity);
        }

        Task<int> IAdminRepository.UpdateAsync(string idColumn, Employees entity)
        {
            return _employeeGenRepository.UpdateAsync(idColumn, entity);
        }

        Task<int> IAdminRepository.DeleteAsync(string idColumn, int id)
        {
            return _employeeGenRepository.DeleteAsync(idColumn, id);
        }

        // Fetch Employee Attendance
        Task<IEnumerable<dynamic>> IAdminRepository.GetAttendanceByMonthYearAsync(int employee, int month, int year)
        {
            return _employeeAttendance.GetAttendanceByMonthYearAsync(employee, month, year);
        }
       

        // Get Attendance By Id or Date
        Task<IEnumerable<Attendance>> IAdminRepository.GetAttendanceByIdAsync(string idColumn, object value)
        {
            return _employeeAttendance.GetAttendanceByIdAsync(idColumn, value);
        }

    }
}
