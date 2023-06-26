using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Account
{
    public class RegisterSaveModel
    {
        public IFormFile? ProfilePictureFile { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The First Name field should have a max of 255 characters.")]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The Last Name field should have a max of 255 characters.")]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
