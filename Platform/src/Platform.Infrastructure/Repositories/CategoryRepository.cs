using Microsoft.EntityFrameworkCore;
using Platform.Application.IRepos;
using Platform.Core.Interfaces;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;

namespace Platform.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CourseDbContext _context;

        public CategoryRepository(CourseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Courses)
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
