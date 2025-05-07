using AMS.Models;
using Azure.Core;

namespace AMS.Interfaces
{
    public interface IHolidayQuotaRepository
    {
       Task ApplyQuotaAsync(List<int> allEmp, int year, int totalHolidays);

        Task<IEnumerable<EmployeeHolidayQuota>> GetQuotasByYearAsync(string idColumn, int year);

    }
}
