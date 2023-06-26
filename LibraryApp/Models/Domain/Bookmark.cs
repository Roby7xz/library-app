using LibraryApp.Areas.Identity.Data;

namespace LibraryApp.Models.Domain
{
    public class Bookmark
    {

        public int Id { get; set; }

        public int BookId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}