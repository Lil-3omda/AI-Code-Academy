using Microsoft.EntityFrameworkCore;
using Platform.Application.IRepos;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Infrastructure.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CourseDbContext _db;
        public CourseRepository(CourseDbContext db) => _db = db;

        public async Task<IEnumerable<Courses>> GetAllAsync()
        {
            // Include Category & Instructor (حسب الحاجة)
            return await _db.Courses
                            .Include(c => c.Category)
                            .Include(c => c.Instructor)
                            .ThenInclude(i => i.User)
                            .ToListAsync();
        }

        public async Task<Courses?> GetByIdAsync(int id)
        {
            return await _db.Courses
                            .Include(c => c.Category)
                            .Include(c => c.Instructor)
                            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Courses> AddAsync(Courses course)
        {
            var e = (await _db.Courses.AddAsync(course)).Entity;
            await _db.SaveChangesAsync();
            return e;
        }

        public async Task UpdateAsync(Courses course)
        {
            _db.Courses.Update(course);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Courses course)
        {
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Courses.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Courses>> GetCoursesByInstructorId(int instructorId)
        {
            var courses = await _db.Courses
                                   .Where(c => c.InstructorId == instructorId)
                                   .Include(c => c.Category)
                                   .Include(c => c.Instructor)
                                   .Include(c=>c.Enrollments)
                                   .Include(c=>c.Modules)
                                   .ThenInclude(m=>m.Videos)
                                   .ToListAsync();
            return courses;
        }
        public async Task<IEnumerable<Courses>?> GetByCategoryNameAsync(string categoryName)
        {
          return await _db.Courses.Include(c=>c.Category).Include(c=>c.Instructor).Where(c => c.Category.Name == categoryName).ToListAsync();
        }
    }
}
