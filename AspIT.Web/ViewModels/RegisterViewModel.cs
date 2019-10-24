using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AspIT.Web.ViewModels
{
    public class RegisterViewModel
    {
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Remote(action: "IsEmailInUse", controller: "Account")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "Password and confirmation password do not math.")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }
    }
}
