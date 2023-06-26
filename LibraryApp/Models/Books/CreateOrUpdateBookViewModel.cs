using LibraryApp.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Books
{
    public class CreateOrUpdateBookViewModel
    {

        public int? Id { get; set; }

        public string Title { get; set; }

        public string ImageName { get; set; }

        public string Description { get; set; }

        public string Genre { get; set; }

        public int? Pages { get; set; }

        public int? SelectedAuthorId { get; set; }
        
        public List<Author> ListOfAuthors { get; set; }
    }
}
