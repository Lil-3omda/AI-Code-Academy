using System;

namespace Platform.Application.DTOs
{
    // Ensure only one definition of CategoryDto exists in this namespace.
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
    }
}
