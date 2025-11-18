using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace Platform.Application.DTOs
{
    public class VideoUploadDto
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]

        public int VideoArrangement { get; set; }
    }
}
