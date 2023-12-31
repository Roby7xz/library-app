﻿using LibraryApp.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models.Authors
{
    public class CreateOrUpdateAuthorSaveModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        public int? Age { get; set; }
    }
}
