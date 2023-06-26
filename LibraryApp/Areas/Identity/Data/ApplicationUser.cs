using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApp.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace LibraryApp.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public string ProfilePicture { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

}

