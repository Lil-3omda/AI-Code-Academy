using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        //public string VideoUrl { get; set; }
        public string FilePath { get; set; }
        public int Duration { get; set; } // بالثواني
        public int VideoArrangement { get; set; }

        [Required]
        [ForeignKey("Module")]
        public int ModuleId { get; set; }

        // العلاقات
        public virtual Module Module { get; set; }
    }
}
