using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Student
    {
        public int Id { get; set; }

        public bool isBlocked { get; set; }

        public bool isDeleted { get; set; }
        //public DateTime DateOfBirth { get; set; }
        [Required]

        [ForeignKey("User")]

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
