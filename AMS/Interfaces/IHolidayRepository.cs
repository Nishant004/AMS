using AMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Interfaces
{
    public interface IHolidayRepository
    {

        Task AddHolidayAsync(Holidays holiday);
        Task<bool> ExistsAsync(DateTime date);
        Task<int> AddSundaysAsync(int year);

        Task<IEnumerable<Holidays>> GetAllHolidays();
    }
}
