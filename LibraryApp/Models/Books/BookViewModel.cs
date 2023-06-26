using LibraryApp.Models.Domain;

namespace LibraryApp.Models.Books
{
    public class BookViewModel
    {

        public List<Book> Books { get; set; }

        public List<Bookmark> Bookmarks { get; set; }
    }
}
