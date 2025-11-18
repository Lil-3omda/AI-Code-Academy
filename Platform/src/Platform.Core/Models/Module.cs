using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ModuleArrangement { get; set; }
        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        // العلاقات
        public virtual Courses Course { get; set; }
        public virtual ICollection<Video> Videos { get; set; }


    }
}
