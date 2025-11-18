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
    public class ModulesDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ModuleArrangement { get; set; }
        public int CourseId { get; set; }
        public EnrolledCoursesDTO Course { get; set; }
        public List<VideoDto> Videos { get; set; }
    }
}
