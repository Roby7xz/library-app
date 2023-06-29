using LibraryApp.Models.Domain;

namespace LibraryApp.Models.Books
{
    public class FilterBookSaveModel
    {
        public string SearchKeyword { get; set; }

        public int? SelectedBookGenreId { get; set; }

        public int? SelectedAuthorId { get; set; }

    }
}
