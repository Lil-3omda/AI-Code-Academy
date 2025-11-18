using Platform.Application.DTOs;
using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.IRepos
{
    public interface ICourseRepository 

    {
        Task<IEnumerable<Courses>> GetAllAsync();
        Task<Courses?> GetByIdAsync(int id);
        Task<IEnumerable<Courses>?> GetByCategoryNameAsync(string categoryName);
        Task<Courses> AddAsync(Courses course);
        Task UpdateAsync(Courses course);
        Task DeleteAsync(Courses course);
        Task<bool> ExistsAsync(int id);

        Task<IEnumerable<Courses>> GetCoursesByInstructorId(int instructorId);
        //Task<IEnumerable<CourseStatsDto>> GetCourseStatsAsync();
        //Task<IEnumerable<Courses>> GetAllCoursesWithModulesAndVideosAsync();
    }
}
