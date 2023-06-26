using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Account
{
    public class ChangeInfoSaveModel
    {
        public IFormFile? ProfilePictureFile { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The First Name field should have a max of 255 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The Last Name field should have a max of 255 characters.")]
        public string LastName { get; set; }
    }
}
