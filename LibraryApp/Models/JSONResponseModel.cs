using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryApp.Models
{
    public class JSONResponseModel
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }

    }
}
