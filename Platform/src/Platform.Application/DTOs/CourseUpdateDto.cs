using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class CourseUpdateDto
    {
        [Required] 
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }

        [Required] 
        public int CategoryId { get; set; }

        [Required] 
        public int InstructorId { get; set; }
    }
}
