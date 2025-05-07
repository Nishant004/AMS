namespace AMS.Models
{

    
    public class EmployeeHolidayQuota
    {
        public int QuotaID { get; set; }
        public int EmployeeID { get; set; }
        public int Year { get; set; }
        public int TotalHolidays { get; set; }
    }

}


