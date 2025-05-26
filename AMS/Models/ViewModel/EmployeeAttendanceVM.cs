namespace AMS.Models.ViewModel
{
    public class EmployeeAttendanceVM
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public DateTime? AttendanceDate { get; set; }
    }
}
