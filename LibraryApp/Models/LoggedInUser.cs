using LibraryApp.Helpers;

namespace LibraryApp.Models
{
    public class LoggedInUser
    {
        public DateTime LoggedInTime { get; set; }

        public string? Theme { get; set; }

        public List<int>? AuthorsIds { get; set; }

        public  List<int>? BooksIds { get; set; }

        public void SetCurrent()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            httpContext?.Session.SetObject("LoggedInUserSession", this);
        }

        public static LoggedInUser? GetCurrent()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            return httpContext?.Session.GetObject<LoggedInUser>("LoggedInUserSession");
        }
    }
}
