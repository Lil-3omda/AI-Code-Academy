using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class AddModuleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ModuleArrangement { get; set; }
        public int CourseId { get; set; }
    }
}
