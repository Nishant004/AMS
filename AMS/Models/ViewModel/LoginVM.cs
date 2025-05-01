using System.ComponentModel.DataAnnotations;

namespace AMS.Models.ViewModel
{
    public class LoginVM
    {

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = null!;

    }
}
