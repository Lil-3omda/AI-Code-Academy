using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Courses
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }

        // العلاقات الأساسية
        public virtual Category Category { get; set; }
        public virtual Instructor Instructor { get; set; }

        public virtual ICollection<Module> Modules { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
