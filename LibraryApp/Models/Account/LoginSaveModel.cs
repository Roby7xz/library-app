using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Account
{
    public class LoginSaveModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public bool RememberMe { get; set; }
    }
}
