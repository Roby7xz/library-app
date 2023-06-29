using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Models.Books;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryApp.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        JSONResponseModel JSON = new JSONResponseModel();

        public BookmarksController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        [Authorize]
        public IActionResult List()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookmarksTable()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = new BookViewModel();

            var bookmarks = await _applicationDbContext.Bookmarks.Where(x => x.User.Id == userId).ToListAsync();

            viewModel.Bookmarks = bookmarks;

            var bookIdList = bookmarks.Select(b => b.BookId).ToList();

            viewModel.Books = await _applicationDbContext.Books.Include("Author")
                              .Include("BookGenre")
                              .Where(b => bookIdList.Contains(b.Id))
                              .ToListAsync();

            return PartialView("_BookmarksTable", viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleBookmark(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookmark = await _applicationDbContext.Bookmarks.FirstOrDefaultAsync(x => x.BookId == bookId && x.User.Id == userId);

            if (bookmark != null)
            {
                _applicationDbContext.Remove(bookmark);
                JSON.Message = "You successfully unbookmarked book!";
            }
            else
            {
                bookmark = new Bookmark();
                bookmark.BookId = bookId;
                bookmark.UserId = userId;
                await _applicationDbContext.Bookmarks.AddAsync(bookmark);
                JSON.Message = "You successfully bookmarked book!";
            }

            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;

            return Json(JSON);
        }
    }
}
