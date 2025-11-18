using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class CourseDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public int InstructorId { get; set; }

        // optional: Category/Instructor names
        public string? CategoryName { get; set; }
        public string? InstructorName { get; set; }
    }
    public class CourseDetailsMoreDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public int InstructorId { get; set; }

        // optional: Category/Instructor names
        public string? CategoryName { get; set; }
        public string? InstructorName { get; set; }

        // Additional Details
        public int NumofModulues { get; set; }
        public int NumofVideos { get; set; }
    }
}
