using LibraryApp.Areas.Identity.Data;
using LibraryApp.Constants;
using LibraryApp.Data;
using LibraryApp.Enums;
using LibraryApp.Helpers;
using LibraryApp.Models;
using LibraryApp.Models.Account;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        JSONResponseModel JSON = new JSONResponseModel();

        public AccountController(
            ApplicationDbContext applicationDbContext,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailStore = GetEmailStore();
        }

        /*** REGISTER SECTION ***/

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterSaveModel register)
        {

            if (!ModelState.IsValid)
            {

                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            if (_userStore.FindByNameAsync(register.Email, CancellationToken.None).Result?.Email != null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User with that Email exist, choose another Email!";
                return Json(JSON);
            }

            if (!FileHelper.IsContentTypeValid(register.ProfilePictureFile))
            {
                JSON.StatusCode = 1;
                JSON.Message = "Please select a valid image format.";
                return Json(JSON);
            }

            var user = new ApplicationUser();

            user.ProfilePicture = FileHelper.UploadFile(PathConstants.ProfilesFolderPath, register.ProfilePictureFile) ?? "/images/profiles/default-profile-image.png";
            user.FirstName = register.FirstName;
            user.LastName = register.LastName;

            await _userStore.SetUserNameAsync(user, register.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, register.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Your registration was unsuccessful.";
                return Json(JSON);
            }

            await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            await _signInManager.SignInAsync(user, isPersistent: false);

            var currentSessionObject = LoggedInUser.GetCurrent();

            if (currentSessionObject == null)
            {
                currentSessionObject = new LoggedInUser();
            }

            currentSessionObject.LoggedInTime = DateTime.Now;

            currentSessionObject.SetCurrent();

            JSON.StatusCode = 0;
            JSON.Message = "Your registration was successful.";
            return Json(JSON);
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        /*** LOGIN SECTION ***/
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginSaveModel login)
        {
            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            if (_userStore.FindByNameAsync(login.Email, CancellationToken.None).Result?.Email == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Incorrect password or email, try again!";
                return Json(JSON);
            }

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Incorrect password or email, try again!";
                return Json(JSON);
            }

            var currentSessionObject = LoggedInUser.GetCurrent();

            if (currentSessionObject == null)
            {
                currentSessionObject = new LoggedInUser();
            }

            currentSessionObject.LoggedInTime = DateTime.Now;

            currentSessionObject.SetCurrent();

            JSON.StatusCode = 0;
            JSON.Message = "You successfully logged in!";
            return Json(JSON);
        }

        [HttpPost]
        public IActionResult ToggleTheme(string theme)
        {
            var currentSessionObject = LoggedInUser.GetCurrent();

            if (currentSessionObject == null)
            {
                currentSessionObject = new LoggedInUser();
            }
            
            currentSessionObject.Theme = theme;               

            currentSessionObject.SetCurrent();

            return Json(new { status = 0, message = "You sucessfully changed theme." });
        }

        /*** LOG OUT SECTION ***/

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        /*** MANAGE SECTION ***/
        [HttpGet]
        [Authorize]
        public IActionResult Manage()
        {
            return View();
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangeAccountPassword()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangeAccountEmail()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeAccountInfo()
        {
            var viewModel = new ChangeInfoViewModel();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User not found.";
                return Json(JSON);
            }

            viewModel.ProfilePicture = user.ProfilePicture;
            viewModel.FirstName = user.FirstName;
            viewModel.LastName = user.LastName;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeAccountPassword(ChangePasswordSaveModel model)
        {
            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User not found.";
                return Json(JSON);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "You can't change you password.";
                return Json(JSON);
            }

            await _signInManager.RefreshSignInAsync(user);

            JSON.StatusCode = 0;
            JSON.Message = "You sucessfully changed your password.";
            return Json(JSON);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeAccountEmail(ChangeEmailSaveModel model)
        {
            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User not found.";
                return Json(JSON);
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);

            if (token == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Server error, cant't generate token.";
                return Json(JSON);
            }

            var changeEmailResult = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);

            if (changeEmailResult.Succeeded)
            {
                user.UserName = model.NewEmail;
                user.NormalizedUserName = model.NewEmail.ToUpper();
                await _userManager.UpdateAsync(user);
                JSON.StatusCode = 0;
                JSON.Message = "You sucessfully changed your email.";
            }
            else
            {
                JSON.StatusCode = 1;
                JSON.Message = "You can't change you email.";
                return Json(JSON);
            }

            await _signInManager.RefreshSignInAsync(user);


            return Json(JSON);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeAccountInfo(ChangeInfoSaveModel model)
        {

            if (!ModelState.IsValid)
            {
                JSON.StatusCode = 1;
                JSON.Message = string.Join(" ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));

                return Json(JSON);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User doesn't exist";
                return Json(JSON);
            }

            if (!FileHelper.IsContentTypeValid(model?.ProfilePictureFile))
            {
                JSON.StatusCode = 1;
                JSON.Message = "Please select a valid image format.";
                return Json(JSON);
            }

            if(model.ProfilePictureFile != null)
            {
                FileHelper.DeleteFile(user.ProfilePicture);
                user.ProfilePicture = FileHelper.UploadFile(PathConstants.ProfilesFolderPath, model.ProfilePictureFile);
            }
                   
            user.FirstName = model.FirstName; 
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "An error occured during your request, please try again later.";
                return Json(JSON);
            }

            await _signInManager.RefreshSignInAsync(user);

            JSON.StatusCode = 0;
            JSON.Message = "You successfully changed your profile informations.";
            return Json(JSON);

        }

        // SUPERADMIN SECTION
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult UsersList()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUsersTable()
        {
            var viewModel = new UsersRolesViewModel();

            viewModel.Users = await _userManager.Users.ToListAsync();
            viewModel.Roles = await _roleManager.Roles.ToListAsync();
            viewModel.UserRolesList = await _applicationDbContext.UserRoles.ToListAsync();


            return PartialView("_UsersTable", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ChangeUserRole(string UserId, string RoleId)
        {

            var userToChange = await _userStore.FindByIdAsync(UserId, CancellationToken.None);

            if (userToChange == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User doesn't exist.";
                return Json(JSON);
            }

            var currentRoleList = await _userManager.GetRolesAsync(userToChange);

            foreach (var currentRole in currentRoleList)
            {
                await _userManager.RemoveFromRoleAsync(userToChange, currentRole);
            }

            var role = await _roleManager.FindByIdAsync(RoleId);

            var result = await _userManager.AddToRoleAsync(userToChange, role.Name);

            if(!result.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "You can't change user role.";
                return Json(JSON);
            }

            JSON.StatusCode = 0;
            JSON.Message = "User role successfully updated.";
            return Json(JSON);            
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var userToChange = await _userStore.FindByIdAsync(Id, CancellationToken.None);

            if (userToChange == null)
            {
                JSON.StatusCode = 1;
                JSON.Message = "User doesn't exist.";
                return Json(JSON);
            }

            var currentRoleList = await _userManager.GetRolesAsync(userToChange);

            foreach (var currentRole in currentRoleList)
            {
                await _userManager.RemoveFromRoleAsync(userToChange, currentRole);
            }

            var result = await _userManager.DeleteAsync(userToChange);

            if (!result.Succeeded)
            {
                JSON.StatusCode = 1;
                JSON.Message = "Failed to delete the user.";
                return Json(JSON);
            }

            if (userToChange.ProfilePicture != null)
            {
                FileHelper.DeleteFile(userToChange.ProfilePicture);
            }

            JSON.StatusCode = 0;
            JSON.Message = "User successfully deleted.";
            return Json(JSON);
        }
    }
}

