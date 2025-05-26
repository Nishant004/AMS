namespace AMS.Models.ViewModel
{
    public class EmployeeDetailsViewModel
    {
        public Employees Employee { get; set; }
        public List<Attendance> AttendanceRecord { get; set; }

        public List<Holidays> HolidayList { get; set; }

    }
}
