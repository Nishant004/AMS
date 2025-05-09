using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Repository
{
    public class HolidayRepository: IHolidayRepository
    {


        private readonly IGenericRepository<Holidays> _holiday;

        public HolidayRepository(IGenericRepository<Holidays> holiday)
        {
            _holiday = holiday;
        }

        public async Task AddHolidayAsync(Holidays holiday)
        {
         
             await _holiday.InsertAsync(holiday);

        }

        public async Task<int> AddSundaysAsync(int year)
        {

            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (!await _holiday.ExistsAsync(date))
                    {
                        var holiday = new Holidays
                        {
                            HolidayDate = date,
                            HolidayName = "Sunday",
                            Description = "Weekend"
                        };

                        await _holiday.InsertAsync(holiday);
                    }
                }

            }
            return 1;
        }

        public async Task<bool> ExistsAsync(DateTime date)
        {
            return await _holiday.ExistsAsync(date);
        }

        public async Task<IEnumerable<Holidays>> GetAllHolidays()

        {

            return await _holiday.GetAllAsync();

        }
    }
}
