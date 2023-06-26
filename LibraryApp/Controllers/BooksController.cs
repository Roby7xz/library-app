using LibraryApp.Constants;
using LibraryApp.Data;
using LibraryApp.Helpers;
using LibraryApp.Models;
using LibraryApp.Models.Books;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;

namespace LibraryApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        JSONResponseModel JSON = new JSONResponseModel();

        public BooksController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        public IActionResult List()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetBooksTable()
        {
            var viewModel = new BookViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            viewModel.Bookmarks = await _applicationDbContext.Bookmarks.Where(x => x.User.Id == userId).ToListAsync();
            viewModel.Books = await _applicationDbContext.Books.Include("Author").ToListAsync();

            return PartialView("_BooksTable", viewModel);
        }

        [HttpGet]
        
        public async Task<IActionResult> Info(int Id)
        {
            var book = Id > 0 ? await _applicationDbContext.Books.Include("Author").FirstOrDefaultAsync(x => x.Id == Id) : null;
           
            if (book == null)
            {
                return RedirectToAction("List");
            }

            var currentSessionObject = LoggedInUser.GetCurrent();

            if(currentSessionObject == null)
            {
                currentSessionObject = new LoggedInUser();
            }
            
            if(currentSessionObject.BooksIds == null)
            {
                currentSessionObject.BooksIds = new List<int>();
            }
            
            if(!currentSessionObject.BooksIds.Contains(book.Id))
            {
                currentSessionObject.BooksIds.Add(book.Id);
            }

            currentSessionObject.SetCurrent();
                     
            return PartialView("_BookInformationModal", book);
        }

        [HttpGet]
        
        public async Task<IActionResult> GetViewedBooks()
        {
            var booksIds = LoggedInUser.GetCurrent()?.BooksIds; 

            var viewModel = new List<Book>();

            if(booksIds != null && booksIds.Count != 0)
            {
                viewModel = await _applicationDbContext.Books.Include("Author").Where(b => booksIds.Contains(b.Id)).ToListAsync();
            }

            return PartialView("_ViewedBooksTable", viewModel);
        }


        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> CreateOrUpdateBook(int Id)
        {
            var viewBook = new CreateOrUpdateBookViewModel();           

            var book = Id > 0 ? await _applicationDbContext.Books.FirstOrDefaultAsync(x => x.Id == Id) : null;

            if (Id > 0 && book == null)
            {
                return RedirectToAction("Error", "Home");
            }

            viewBook.ImageName = book?.ImageName;
            viewBook.Id = book?.Id;
            viewBook.Title = book?.Title;
            viewBook.Description = book?.Description;
            viewBook.Genre = book?.Genre;
            viewBook.Pages = book?.Pages;
            viewBook.SelectedAuthorId = book?.AuthorId;
            viewBook.ListOfAuthors = _applicationDbContext.Authors.ToList();

            return PartialView("_CreateOrUpdateBookModal", viewBook);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> CreateOrUpdateBook(CreateOrUpdateBookSaveModel createOrUpdateSaveModel)
        {

            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            var author = await _applicationDbContext.Authors.FindAsync(createOrUpdateSaveModel.SelectedAuthorId);
            if (author == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "This author does not exist.";

                return Json(JSON);
            }

            if (author.Age < 18)
            {
                JSON.StatusCode = 1;
                JSON.Message = "This author is under 18, choose another one.";

                return Json(JSON);
            }

            var book = createOrUpdateSaveModel.Id > 0 ? await _applicationDbContext.Books.FindAsync(createOrUpdateSaveModel.Id) : null;
            if (book == null)
            {
                book = new Book();
                book.ImageName = PathConstants.BooksFolderPath + "/default-book-cover.jpg";
                await _applicationDbContext.AddAsync(book);
            }
         
            if (!FileHelper.IsContentTypeValid(createOrUpdateSaveModel.Image))
            {
                JSON.StatusCode = 1;
                JSON.Message = "Please select a valid image format.";
                return Json(JSON);
            }

            if (createOrUpdateSaveModel.Image != null)
            {

                if(book.ImageName != PathConstants.BooksFolderPath + "/default-book-cover.jpg")
                {
                    FileHelper.DeleteFile(book.ImageName);
                }
                
                book.ImageName = FileHelper.UploadFile(PathConstants.BooksFolderPath, createOrUpdateSaveModel.Image);
            }
            
            book.Title = createOrUpdateSaveModel.Title;
            book.Description = createOrUpdateSaveModel.Description;
            book.Genre = createOrUpdateSaveModel.Genre;
            book.Pages = createOrUpdateSaveModel.Pages != null ? createOrUpdateSaveModel.Pages.Value : 0;
            book.Author = author;
            
            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You have successfully modified the book!";

            return Json(JSON);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete(int Id)
        {           
            var book = await _applicationDbContext.Books.FindAsync(Id);

            if (book == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Book not found!";
                return Json(JSON);               
            }

            if (book.ImageName != null)
            {
                FileHelper.DeleteFile(book.ImageName);
            }

            _applicationDbContext.Books.Remove(book);
            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You successfully deleted the book.";
            return Json(JSON);
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
                JSON.Message = "You successfully unbookmarked book!" ;
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

        // Show Bookmarks
        [HttpGet]
        [Authorize]
        public IActionResult BookmarksList()
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

            viewModel.Books = await _applicationDbContext.Books.Include("Author").Where(b => bookIdList.Contains(b.Id)).ToListAsync();

            return PartialView("_BookmarksTable", viewModel);
        }
    }
}

