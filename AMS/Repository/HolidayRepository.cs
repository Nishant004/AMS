using AMS.Data;
using AMS.Interfaces;
using AMS.Models;

namespace AMS.Repository
{
    public class HolidayRepository:IHolidayRepository
    {


        private readonly IGenericRepository<Holidays> _holiday;

        public HolidayRepository(IGenericRepository<Holidays> holiday)
        {
            _holiday = holiday;
        }




    }
}
