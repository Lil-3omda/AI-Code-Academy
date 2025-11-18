using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Core.Interfaces.IRepos;

namespace Platform.Infrastructure.Repositories
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly CourseDbContext _context;

        public InstructorRepository(CourseDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Instructor instructor) =>
            await _context.Instructors.AddAsync(instructor);

        public async Task<Instructor?> GetByIdAsync(int id) =>
            await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Instructor>> GetAllAsync() =>
            await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .ToListAsync();

        public Task UpdateAsync(Instructor instructor)
        {
            _context.Instructors.Update(instructor);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();

    }
}
