using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Account
{
    public class ChangeEmailSaveModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
