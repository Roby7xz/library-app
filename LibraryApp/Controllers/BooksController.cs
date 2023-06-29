using LibraryApp.Constants;
using LibraryApp.Data;
using LibraryApp.Helpers;
using LibraryApp.Models;
using LibraryApp.Models.Books;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var filterModel = new FilterBookViewModel();

            filterModel.ListOfAuthors = _applicationDbContext.Authors.ToList();
            filterModel.ListOfBookGenres = _applicationDbContext.BookGenres.ToList();

            return View(filterModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetBooksTable([FromQuery] FilterBookSaveModel filterBookModel)
        {
            var viewModel = new BookViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var filteredBooks = await _applicationDbContext.Books.Include("Author").Include("BookGenre").ToListAsync();

            var filteredBookIds = filteredBooks.Select(b => b.Id); 

            if (filterBookModel.SelectedAuthorId != null)
            {
                filteredBooks = filteredBooks.Where(b => b.AuthorId == filterBookModel.SelectedAuthorId).ToList();
            }

            if (filterBookModel.SelectedBookGenreId != null)
            {
                filteredBooks = filteredBooks.Where(b => b.BookGenreId == filterBookModel.SelectedBookGenreId).ToList();
            }

            if (!string.IsNullOrEmpty(filterBookModel.SearchKeyword))
            {
                filteredBooks = filteredBooks.Where(b => b.Title.Contains(filterBookModel.SearchKeyword)).ToList();
            }        

            viewModel.Books = filteredBooks;
            viewModel.Bookmarks = await _applicationDbContext.Bookmarks
                                 .Where(x => x.User.Id == userId && filteredBookIds.Contains(x.BookId))
                                 .ToListAsync();

            return PartialView("_BooksTable", viewModel);
        }

        [HttpGet]      
        public async Task<IActionResult> Info(int Id)
        {
            var book = Id > 0 ? await _applicationDbContext.Books.Include("Author").Include("BookGenre").FirstOrDefaultAsync(x => x.Id == Id) : null;
           
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
                viewModel = await _applicationDbContext.Books.Include("Author")
                            .Include("BookGenre")
                            .Where(b => booksIds.Contains(b.Id))
                            .ToListAsync();
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
            viewBook.Pages = book?.Pages;
            viewBook.SelectedBookGenreId = book?.BookGenreId;
            viewBook.ListOfBookGenres = _applicationDbContext.BookGenres.ToList();
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
                book.ImageName = "/images/books/default-book-cover.jpg";
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

                if(book.ImageName != "/images/books/default-book-cover.jpg")
                {
                    FileHelper.DeleteFile(book.ImageName);
                }
                
                book.ImageName = FileHelper.UploadFile(PathConstants.BooksFolderPath, createOrUpdateSaveModel.Image);
            }
            
            book.Title = createOrUpdateSaveModel.Title;
            book.Description = createOrUpdateSaveModel.Description;
            book.BookGenreId = createOrUpdateSaveModel.SelectedBookGenreId;
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

            var bookmark = await _applicationDbContext.Bookmarks.FirstOrDefaultAsync(b => b.BookId == book.Id);

            _applicationDbContext.Bookmarks.Remove(bookmark);
            _applicationDbContext.Books.Remove(book);
            
            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You successfully deleted the book.";
            return Json(JSON);
        }

    }
}

