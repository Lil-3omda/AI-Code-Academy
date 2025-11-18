using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class AddEnrollmentDTO
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public decimal ProgressPercentage { get; set; }

        public int StdId { get; set; }

        public int CourseId { get; set; }
      
        public string Status { get; set; } = "Pending";
    }
}
