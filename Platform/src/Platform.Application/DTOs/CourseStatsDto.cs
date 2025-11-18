using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class CourseStatsDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int ModuleCount { get; set; }
        public int VideoCount { get; set; }
    }
}
