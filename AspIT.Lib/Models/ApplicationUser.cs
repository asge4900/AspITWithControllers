using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspIT.Lib.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
    }
}
