using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class EnrollmentsByInstructorDTO
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public decimal ProgressPercentage { get; set; }

        public int StdId { get; set; }

        public int CourseId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsCanceled { get; set; } = false;
        public string Status { get; set; } 

    
        public EnrolledCoursesDTO Course { get; set; }
    }
}
