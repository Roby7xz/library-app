using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Books
{
    public class CreateOrUpdateBookSaveModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "Number of pages is required.")]
        public int? Pages { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        public int? SelectedAuthorId { get; set; }
    }
}
