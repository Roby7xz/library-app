using LibraryApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace LibraryApp.Models.Account
{
    public class UsersRolesViewModel
    {
        public List<ApplicationUser> Users { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public List<IdentityUserRole<string>> UserRolesList { get; set; }
    }
}
