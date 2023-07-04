using LibraryApp.Models.Books;
using LibraryApp.Models.Domain;

namespace LibraryApp.Helpers
{
    public class FilterHelper
    {

        public static List<Book> FilterBooks(List<Book> books, FilterBookSaveModel filterBookModel)
        {

            if (filterBookModel.SelectedAuthorId != null)
            {
                books = books.Where(b => b.AuthorId == filterBookModel.SelectedAuthorId).ToList();
            }

            if (filterBookModel.SelectedBookGenreId != null)
            {
                books = books.Where(b => b.BookGenreId == filterBookModel.SelectedBookGenreId).ToList();
            }

            if (!string.IsNullOrEmpty(filterBookModel.SearchKeyword))
            {
                books = books.Where(b => b.Title.Contains(filterBookModel.SearchKeyword)).ToList();
            }

            return books;
        }

    }
}
