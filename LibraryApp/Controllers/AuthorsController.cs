using LibraryApp.Data;
using LibraryApp.Helpers;
using LibraryApp.Models;
using LibraryApp.Models.Authors;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        JSONResponseModel JSON = new JSONResponseModel();

        public AuthorsController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var listOfAuthors = await _applicationDbContext.Authors.ToListAsync();
            return View(listOfAuthors);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateOrUpdateAuthor(int Id)
        {
            var author = Id > 0 ? await _applicationDbContext.Authors.FindAsync(Id) : null;
            var viewModel = new CreateOrUpdateAuthorViewModel();

            if (Id > 0 && author == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if(author != null)
            {
                viewModel.Author = author;

                var currentSessionObject = LoggedInUser.GetCurrent();

                if (currentSessionObject == null)
                {
                    currentSessionObject = new LoggedInUser();
                }

                if (currentSessionObject.AuthorsIds == null)
                {
                    currentSessionObject.AuthorsIds = new List<int>();
                }

                if (!currentSessionObject.AuthorsIds.Contains(author.Id))
                {
                    currentSessionObject.AuthorsIds.Add(author.Id);
                }

                currentSessionObject.SetCurrent();
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetViewedAuthors()
        {
            var authorsIds = LoggedInUser.GetCurrent()?.AuthorsIds;

            var viewModel = new List<Author>();

            if (authorsIds != null && authorsIds.Count != 0)
            {
                viewModel = await _applicationDbContext.Authors.Where(b => authorsIds.Contains(b.Id)).ToListAsync();
            }

            return PartialView("_ViewedAuthorsTable", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> CreateOrUpdateAuthor(CreateOrUpdateAuthorSaveModel createOrUpdateAuthorSaveModel)
        {
            var author = createOrUpdateAuthorSaveModel.Id > 0 ? await _applicationDbContext.Authors.FindAsync(createOrUpdateAuthorSaveModel.Id) : null;

            if(!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            if(author == null)
            {
                author = new Author();
                await _applicationDbContext.Authors.AddAsync(author);
            }

            author.Name = createOrUpdateAuthorSaveModel.Name;
            author.Age = createOrUpdateAuthorSaveModel.Age;

            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You have successfully modified the author!";
            return Json(JSON);     
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete(int Id)
        {
            var author = await _applicationDbContext.Authors.FindAsync(Id);
            var book = await _applicationDbContext.Books.FirstOrDefaultAsync(x => x.AuthorId == Id);

            if (book != null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "You can't delete this author.";
                return Json(JSON);
            }

            _applicationDbContext.Remove(author);
            await _applicationDbContext.SaveChangesAsync();

            JSON.StatusCode = 0;
            JSON.Message = "You successfully deleted author!";
            return Json(JSON);
        }    
    }
}
