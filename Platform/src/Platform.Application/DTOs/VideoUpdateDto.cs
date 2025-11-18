using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Platform.Application.DTOs
{
    public class VideoUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ModuleId { get; set; }
        public int VideoArrangement { get; set; }
        public IFormFile? File { get; set; } // optional new video file
    }
}
