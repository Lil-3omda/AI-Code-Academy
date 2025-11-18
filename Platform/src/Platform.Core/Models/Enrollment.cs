using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public decimal ProgressPercentage { get; set; }
        [Required]
        [ForeignKey("Student")]
        public int StdId { get; set; }
        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public bool IsDeleted { get; set; }=false;
        public bool IsCanceled { get; set; } = false;
        public string Status { get; set; } = "Pending";

        // العلاقات
        public virtual Student Student { get; set; }
        public virtual Courses Course { get; set; }
    }
}
