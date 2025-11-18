using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string LinkedIn { get; set; }
        public bool IsVerified { get; set; }

        public string Qualifications { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public virtual ICollection<Courses> Courses { get; set; }
    }
}
