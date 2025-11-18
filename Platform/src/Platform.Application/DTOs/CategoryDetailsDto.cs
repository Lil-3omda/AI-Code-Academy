using Platform.Core.DTOs;
using System.Collections.Generic;

namespace Platform.Application.DTOs
{
    public class CategoryDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // List of courses in this category
        public int CourseCount { get; set; }
    }
}
