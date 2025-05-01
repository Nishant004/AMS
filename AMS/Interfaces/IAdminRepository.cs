using AMS.Models;

namespace AMS.Interfaces
{
    public interface IAdminRepository
    {
        // Crud for employee
        Task<IEnumerable<Employees>> GetAllAsync(); 
        Task<Employees> GetByIdAsync(string idColumn, int id); 
        Task<int> InsertAsync(Employees entity); 
        Task<int> UpdateAsync(string idColumn, Employees entity); 
        Task<int> DeleteAsync(string idColumn, int id); 

     

         Task<(User? user, bool isDeactivated)> GetByUserCredentialsAsync(
         string usernameColumn, string passwordColumn, string roleColumn,
         string username, string password, string role);


        // Fetch Employee Attendance
        Task<IEnumerable<dynamic>> GetAttendanceByMonthYearAsync(int employee, int month, int year);

        // Get Attendance By Id or Date
        Task<IEnumerable<Attendance>> GetAttendanceByIdAsync(string idColumn, object value);

    }
}

