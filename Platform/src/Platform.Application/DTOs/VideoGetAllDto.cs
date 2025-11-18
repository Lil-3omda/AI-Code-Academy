using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class VideoGetAllDto
    {
            public int Id { get; set; }
            public string Title { get; set; }
            public string FilePath { get; set; }
            public int Duration { get; set; }
            public int VideoArrangement { get; set; }
            public int ModuleId { get; set; }

            // new fields
            public int CourseId { get; set; }
            public string CourseName { get; set; }
    }
}
