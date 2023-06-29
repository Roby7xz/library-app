using LibraryApp.Data;
using LibraryApp.Enums;
using LibraryApp.Models;
using LibraryApp.Models.Domain;
using LibraryApp.Models.Genres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Controllers
{
    public class GenresController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        JSONResponseModel JSON = new JSONResponseModel();

        public GenresController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        public IActionResult List()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetGenresTable()
        {
            var bookGenres = await _applicationDbContext.BookGenres.ToListAsync();
            return PartialView("_GenresTable", bookGenres);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> CreateOrUpdateGenre(int Id)
        {
            var viewModel = new CreateOrUpdateGenreModel();
            var bookGenre = Id > 0 ? await _applicationDbContext.BookGenres.FirstOrDefaultAsync(x => x.Id == Id) : null;

            if (Id > 0 && bookGenre == null)
            {
                return RedirectToAction("Error", "Home");
            }

            viewModel.Id = bookGenre?.Id;
            viewModel.Name = bookGenre?.Name;

            return PartialView("_CreateOrUpdateGenreModal", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> CreateOrUpdateGenre(CreateOrUpdateGenreModel model)
        {

            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            var bookGenre = model.Id > 0 ? await _applicationDbContext.BookGenres.FindAsync(model.Id) : null;

            if (bookGenre == null)
            {
                bookGenre = new BookGenre();
                await _applicationDbContext.AddAsync(bookGenre);
            }

            bookGenre.Name = model.Name;

            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You have successfully modified the book genre!";

            return Json(JSON);

        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete(int Id)
        {
            var bookGenre = await _applicationDbContext.BookGenres.FirstOrDefaultAsync(bg => bg.Id == Id);

            if (bookGenre == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "No book genre found!";
                return Json(JSON);
            }

            var isGenreConnectedWithBook = await _applicationDbContext.Books.AnyAsync(b => b.BookGenreId == Id);

            if (isGenreConnectedWithBook)
            {
                JSON.StatusCode = 1;
                JSON.Message = "There are books with this genre, please delete the book first.";
                return Json(JSON);
            }

            _applicationDbContext.BookGenres.Remove(bookGenre);
            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You successfully deleted book genre.";
            return Json(JSON);
        }
    }
}
