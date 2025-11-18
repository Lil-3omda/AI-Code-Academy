using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Platform.Core.Models
{
    public class AppUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }


        //public DateTime DateOfBirth { get; set; }
        public string?Address { get; set; }

        public string? Gender { get; set; }



    }
}
