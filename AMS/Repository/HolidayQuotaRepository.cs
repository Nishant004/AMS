using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using NuGet.Protocol.Plugins;

namespace AMS.Repository
{
    public class HolidayQuotaRepository : IHolidayQuotaRepository
    {
       private readonly IGenericRepository<EmployeeHolidayQuota> _holidayQuotaRepository;

        public HolidayQuotaRepository(IGenericRepository<EmployeeHolidayQuota> holidayQuotaRepository)
        {
            _holidayQuotaRepository = holidayQuotaRepository;
        }

        public async Task ApplyQuotaAsync(List<int> allEmp, int year, int totalHolidays)
        {
            var tasks = allEmp.Select(async empId =>
            {
                var existingQuota = await _holidayQuotaRepository.GetByEmployeeAndYearAsync(empId, year);

                if (existingQuota != null) // Update if exists
                {
                    existingQuota.TotalHolidays = totalHolidays;
                    var idColumn = "QuotaID"; // Use correct primary key
                    await _holidayQuotaRepository.UpdateAsync(idColumn, existingQuota);
                }
                else // Insert new if not exists
                {
                    var quota = new EmployeeHolidayQuota
                    {
                        EmployeeID = empId,
                        Year = year,
                        TotalHolidays = totalHolidays
                    };

                    await _holidayQuotaRepository.InsertAsync(quota);
                }
            });

            await Task.WhenAll(tasks);
        

        }

        public async Task<IEnumerable<EmployeeHolidayQuota>> GetQuotasByYearAsync(string idColumn, int year)
        {
            return await _holidayQuotaRepository.GetAttendanceByIdAsync(idColumn, year);
        }

    }
}
