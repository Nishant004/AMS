using System.ComponentModel.DataAnnotations;

namespace AMS.Models.ViewModel
{
    public class UserCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
