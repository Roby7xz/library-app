
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Domain
{
    public class Book
    {
        
        public int Id { get; set; }

        public string ImageName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Pages { get; set; }

        public int? AuthorId { get; set; }

        public virtual Author? Author { get; set; }

        public int? BookGenreId { get; set; }

        public virtual BookGenre? BookGenre { get; set; }

    }
}
