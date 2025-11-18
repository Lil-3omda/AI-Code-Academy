using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }

        public string Permissions { get; set; } // JSON-formatted string

        // Navigation property
        public AppUser User { get; set; }
    }
}
