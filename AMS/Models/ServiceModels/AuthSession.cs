namespace AMS.Models.ServiceModels
{
    public class AuthSession
    {
        public int UserId { get; set; }

        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string Role { get; set; }
    }
}
