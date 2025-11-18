using System.ComponentModel.DataAnnotations;

namespace Platform.Application.DTOs
{
    public class CategoryUpdateDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
