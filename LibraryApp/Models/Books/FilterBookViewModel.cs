using LibraryApp.Models.Domain;

namespace LibraryApp.Models.Books
{
    public class FilterBookViewModel
    {

        public int? SelectedBookGenreId { get; set; }

        public int? SelectedAuthorId { get; set; }

        public List<BookGenre> ListOfBookGenres { get; set; }

        public List<Author> ListOfAuthors { get; set; }
    }
}
